using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScrumPokerClub.Services.Requests;
using ScrumPokerClub.Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScrumPokerClub.Data;
using Snowflake.Core;

namespace ScrumPokerClub.Services
{
    /// <summary>
    /// Default implementation of Spc session management.
    /// </summary>
    class SessionManagementService : ISessionManagementService
    {
        /// <summary>
        /// A map of sessions to their names.
        /// </summary>
        private readonly IDictionary<string, ISessionEvents> sessions;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The service provider.
        /// </summary>
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// The snowflake provider.
        /// </summary>
        private readonly ISnowflakeProvider snowflakeProvider;

        /// <summary>
        /// The structured data store.
        /// </summary>
        private readonly IDatabase dataStore;

        /// <summary>
        /// A random instance.
        /// </summary>
        private static readonly Random random = new();

        /// <summary>
        /// Create a new instance of <see cref="SessionManagementService"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="dataStore">The structured data store.</param>
        public SessionManagementService(
            ILogger<SessionManagementService> logger,
            IServiceProvider serviceProvider,
            ISnowflakeProvider snowflakeProvider,
            IDatabase dataStore)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.dataStore = dataStore;
            this.snowflakeProvider = snowflakeProvider;

            sessions = new Dictionary<string, ISessionEvents>();
        }

        /// <inheritdoc/>
        public async Task EnsureSessionConfiguredAsync(ConfigureSessionRequest configureSessionRequest)
        {
            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            if (!sessions.TryGetValue(configureSessionRequest.Session, out ISessionEvents session))
            {
                session = new SessionEvents();
                logger.LogInformation($"Creating event container for session {configureSessionRequest.Session}.");
            }

            configureSessionRequest.Configure?.Invoke(session);

            sessions[configureSessionRequest.Session] = session;

            // if no session exists in the back end by this id, then make it.
            if (await dataStore.GetSessionAsync(configureSessionRequest.Session) is null)
            {
                if (!ulong.TryParse(configureSessionRequest.Session, out ulong id))
                {
                    id = await snowflakeProvider.GetNextAsync();
                }

                await dataStore.UpsertSessionAsync(new Data.Entities.Session()
                {
                    Id = id,
                    Name = configureSessionRequest.Session,
                });
            }

            // lastly, ensure the context for this session exists.
            await dataStore.EnsureSessionContextExistsAsync(configureSessionRequest.Session);
        }

        /// <inheritdoc/>
        public async Task JoinSessionAsync(JoinSessionRequest joinSessionRequest)
        {
            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            // bail if no session.
            if (!sessions.TryGetValue(joinSessionRequest.Session, out ISessionEvents session))
            {
                logger.LogError($"Player {userInfo.Identifier} attempted operation on non-existent session.");
                return;
            }

            // transition session to voting if we're the first to join.
            var playersInSession = await dataStore.GetAllPlayerContextsInSessionContextAsync(joinSessionRequest.Session);
            if (!playersInSession.Any())
                await dataStore.SetGamePhaseAsync(joinSessionRequest.Session, GamePhase.Voting);

            // pull the player context. if it doesn't exist, pull the profile and create it.
            Data.Entities.Profile profile;
            var playerContext = await dataStore.GetPlayerContextAsync(joinSessionRequest.Session, userInfo.Identifier);
            if (playerContext is null)
            {
                // if the profile also doesn't exist, create that too.
                profile = await dataStore.GetProfileAsync(userInfo.Identifier);
                if (profile is null)
                {
                    profile = new Data.Entities.Profile()
                    {
                        Id = userInfo.Identifier,
                        AvatarId = random.Next(1, 20),
                    };
                    await dataStore.UpsertProfileAsync(profile);
                }

                // create and fetch the player context.
                await dataStore.AddToSessionContextAsync(joinSessionRequest.Session, profile.Id, userInfo);
                playerContext = await dataStore.GetPlayerContextAsync(joinSessionRequest.Session, profile.Id);
            }

            // report to the clients that the player has joined.
            await session.RaisePlayerConnectedAsync(new PlayerConnectedResponse()
            {
                Id = playerContext.Profile.Id,
                Name = playerContext.DisplayName,
                AvatarId = playerContext.Profile.AvatarId,
                IsModerator = playerContext.Moderating
            });

            // update the cached session object
            sessions[joinSessionRequest.Session] = session;
        }

        /// <inheritdoc/>
        public async Task<SessionStateResponse> GetSessionStateAsync(SessionStateRequest sessionStateRequest)
        {
            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            // bail if no session.
            if (!sessions.TryGetValue(sessionStateRequest.Session, out ISessionEvents _))
            {
                logger.LogError($"Player {userInfo.Identifier} attempted operation on non-existent session.");
                return default;
            }

            var players = await dataStore.GetAllPlayerContextsInSessionContextAsync(sessionStateRequest.Session);
            var phase = await dataStore.GetGamePhaseAsync(sessionStateRequest.Session);

            return await Task.FromResult(new SessionStateResponse()
            {
                GamePhase = phase,
                Players = players
            });
        }

        /// <inheritdoc/>
        public async Task LeaveSessionAsync(LeaveSessionRequest leaveSessionRequest)
        {
            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            if (!sessions.TryGetValue(leaveSessionRequest.Session, out ISessionEvents session))
            {
                logger.LogError($"Player {userInfo.Identifier} attempted operation on non-existent session.");
                return;
            }

            if (await dataStore.GetPlayerContextAsync(leaveSessionRequest.Session, leaveSessionRequest.Id) is null)
                return;

            await dataStore.RemoveFromSessionContextAsync(leaveSessionRequest.Session, leaveSessionRequest.Id);
            await session.RaisePlayerDisconnectedAsync(new PlayerDisconnectedResponse()
            {
                Id = leaveSessionRequest.Id,
            });
        }

        /// <inheritdoc/>
        public async Task TransitionToPlayPhaseAsync(TransitionToPlayPhaseRequest transitionToPlayPhaseRequest)
        {
            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            if (!sessions.TryGetValue(transitionToPlayPhaseRequest.Session, out ISessionEvents session))
            {
                logger.LogError($"Player {userInfo.Identifier} attempted operation on non-existent session.");
                return;
            }

            var player = await dataStore.GetPlayerContextAsync(transitionToPlayPhaseRequest.Session, userInfo.Identifier);
            if (!player.Moderating)
            {
                logger.LogError($"Non-moderating player '{userInfo.Identifier}' attempted change phase of play.");
                return;
            }

            var gamePhase = await dataStore.GetGamePhaseAsync(transitionToPlayPhaseRequest.Session);
            if (gamePhase != GamePhase.Results)
            {
                logger.LogError($"Player '{userInfo.Identifier}' attempted a transition to play during non-results phase.");
                return;
            }

            var players = await dataStore.GetAllPlayerContextsInSessionContextAsync(transitionToPlayPhaseRequest.Session);
            foreach (var p in players)
                await dataStore.SetVoteAsync(transitionToPlayPhaseRequest.Session, p.Profile.Id, null);

            await dataStore.SetGamePhaseAsync(transitionToPlayPhaseRequest.Session, GamePhase.Voting);
            await session.RaiseTransitionToPlayAsync(new TransitionToPlayResponse()
            {
                // nothing to do
            });
        }

        /// <inheritdoc/>
        public async Task TransitionToResultsPhaseAsync(TransitionToResultsPhaseRequest transitionToResultsPhaseRequest)
        {
            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            if (!sessions.TryGetValue(transitionToResultsPhaseRequest.Session, out ISessionEvents session))
            {
                logger.LogError($"Player {userInfo.Identifier} attempted operation on non-existent session.");
                return;
            }

            var player = await dataStore.GetPlayerContextAsync(transitionToResultsPhaseRequest.Session, userInfo.Identifier);
            if (!player.Moderating)
            {
                logger.LogError($"Non-moderating player {userInfo.Identifier} attempted change phase of play.");
                return;
            }

            var gamePhase = await dataStore.GetGamePhaseAsync(transitionToResultsPhaseRequest.Session);
            if (gamePhase != GamePhase.Voting)
            {
                logger.LogError($"Player {userInfo.Identifier} attempted a transition to results during non-voting phase.");
                return;
            }

            await dataStore.SetGamePhaseAsync(transitionToResultsPhaseRequest.Session, GamePhase.Results);
            var response = TransitionToResultsResponse.FromPlayerStates(
                await dataStore.GetAllPlayerContextsInSessionContextAsync(transitionToResultsPhaseRequest.Session));

            await session.RaiseTransitionToResultsAsync(response);
        }

        /// <inheritdoc/>
        public async Task UpdateParticipantAsync(UpdateParticipantRequest updateParticipantRequest)
        {
            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            if (!sessions.TryGetValue(updateParticipantRequest.Session, out ISessionEvents session))
            {
                logger.LogError($"Player {userInfo.Identifier} attempted operation on non-existent session.");
                return;
            }

            var player = await dataStore.GetPlayerContextAsync(updateParticipantRequest.Session, userInfo.Identifier);
            player.Moderating = updateParticipantRequest.IsModerator ?? player.Moderating;
            player.Profile.AvatarId = updateParticipantRequest.AvatarId ?? player.Profile.AvatarId;

            if (!string.IsNullOrEmpty(updateParticipantRequest.DisplayName))
            {
                if (updateParticipantRequest.DisplayName.Equals("usemymicrosoftname"))
                {
                    // clear *both* display names to force reversion in-app
                    player.DisplayName = userInfo.Name;
                    player.Profile.DisplayName = null;
                }
                else
                {
                    player.Profile.DisplayName = updateParticipantRequest.DisplayName;
                }
            }

            await dataStore.UpsertProfileAsync(player.Profile);
            await session.RaisePlayerUpdatedAsync(new PlayerUpdatedResponse()
            {
                Id = userInfo.Identifier,
                IsModerator = player.Moderating,
                AvatarId = player.Profile.AvatarId,
                DisplayName = player.DisplayName,
            });
        }

        /// <inheritdoc/>
        public async Task UpdateVoteAsync(UpdateVoteRequest updateVoteRequest)
        {
            if (!sessions.TryGetValue(updateVoteRequest.Session, out ISessionEvents session))
                return;

            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            var gamePhase = await dataStore.GetGamePhaseAsync(updateVoteRequest.Session);
            if (gamePhase != GamePhase.Voting)
            {
                logger.LogError($"Player {userInfo.Identifier} attempted to vote during a non-voting phase of play.");
                return;
            }

            if (!int.TryParse(updateVoteRequest.Vote, out int outVote) && !(
                outVote < (int)ModifiedFibonacciVote.Zero ||
                outVote > (int)ModifiedFibonacciVote.Unknown))
            {
                logger.LogError($"Player {userInfo.Identifier} sent an illegal vote.");
                return;
            }

            await dataStore.SetVoteAsync(updateVoteRequest.Session, userInfo.Identifier, outVote);

            await session.RaiseVoteUpdatedAsync(new VoteUpdatedResponse() { Id = userInfo.Identifier });
        }
    }
}
