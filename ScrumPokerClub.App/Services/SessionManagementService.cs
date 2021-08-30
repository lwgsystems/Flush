using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScrumPokerClub.Services.Requests;
using ScrumPokerClub.Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScrumPokerClub.Data;

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
        private readonly IDictionary<string, ISession> sessions;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The service provider.
        /// </summary>
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// The structured data store.
        /// </summary>
        private readonly IDataStore2 dataStore2;

        /// <summary>
        /// Create a new instance of <see cref="SessionManagementService"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="dataStore2">The structured data store.</param>
        public SessionManagementService(
            ILogger<SessionManagementService> logger,
            IServiceProvider serviceProvider,
            IDataStore2 dataStore2)
        {
            logger.LogError("this is an initial error");
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.dataStore2 = dataStore2;

            sessions = new Dictionary<string, ISession>();
        }

        /// <inheritdoc/>
        public async Task EnsureSessionConfiguredAsync(ConfigureSessionRequest configureSessionRequest)
        {
            await Task.CompletedTask;
            if (!sessions.TryGetValue(configureSessionRequest.Session, out ISession session))
                session = new Session();

            configureSessionRequest.Configure?.Invoke(session);

            sessions[configureSessionRequest.Session] = session;
        }

        /// <inheritdoc/>
        public async Task JoinSessionAsync(JoinSessionRequest joinSessionRequest)
        {
            await Task.CompletedTask;
            if (!sessions.TryGetValue(joinSessionRequest.Session, out ISession session))
                return;

            session.Increment();

            if (!dataStore2.AnyPlayersIn(joinSessionRequest.Session))
                dataStore2.SetGamePhase(joinSessionRequest.Session, GamePhase.Voting);

            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            if (dataStore2.GetPlayerState(userInfo.Identifier) is null)
                dataStore2.AddPlayer(userInfo.Identifier, userInfo.Name, joinSessionRequest.Session);
            else
                dataStore2.SetConnectionState(userInfo.Identifier, true);

            var player = dataStore2.GetPlayerState(userInfo.Identifier);
            await session.RaisePlayerConnectedAsync(new PlayerConnectedResponse()
            {
                Id = userInfo.Identifier,
                Name = userInfo.Name,
                AvatarId = player.AvatarId
            });

            sessions[joinSessionRequest.Session] = session;
        }

        /// <inheritdoc/>
        public async Task<SessionStateResponse> GetSessionStateAsync(SessionStateRequest sessionStateRequest)
        {
            if (!sessions.TryGetValue(sessionStateRequest.Session, out ISession session))
                return default(SessionStateResponse);

            var players = dataStore2.PlayersIn(sessionStateRequest.Session);
            var phase = dataStore2.GetGamePhase(sessionStateRequest.Session);

            return await Task.FromResult(new SessionStateResponse()
            {
                GamePhase = phase,
                Players = players
            });
        }

        /// <inheritdoc/>
        public async Task LeaveSessionAsync(LeaveSessionRequest leaveSessionRequest)
        {
            await Task.CompletedTask;
            if (!sessions.TryGetValue(leaveSessionRequest.Session, out ISession session))
                return;

            if (dataStore2.GetPlayerState(leaveSessionRequest.Id) is null)
                return;

            session.Decrement();

            dataStore2.SetConnectionState(leaveSessionRequest.Id, false);

            await session.RaisePlayerDisconnectedAsync(new PlayerDisconnectedResponse()
            {
                Id = leaveSessionRequest.Id,
            });
        }

        /// <inheritdoc/>
        public async Task TransitionToPlayPhaseAsync(TransitionToPlayPhaseRequest transitionToPlayPhaseRequest)
        {
            await Task.CompletedTask;
            if (!sessions.TryGetValue(transitionToPlayPhaseRequest.Session, out ISession session))
                return;

            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            var player = dataStore2.GetPlayerState(userInfo.Identifier);
            if (!player.IsModerator)
                return;

            var gamePhase = dataStore2.GetGamePhase(transitionToPlayPhaseRequest.Session);
            if (gamePhase != GamePhase.Results)
                return;

            var players = dataStore2.PlayersIn(transitionToPlayPhaseRequest.Session);
            foreach (var p in players)
                dataStore2.SetVote(p.PlayerId, null);

            dataStore2.SetGamePhase(transitionToPlayPhaseRequest.Session, GamePhase.Voting);
            await session.RaiseTransitionToPlayAsync(new TransitionToPlayResponse()
            {
                // nothing to do
            });
        }

        /// <inheritdoc/>
        public async Task TransitionToResultsPhaseAsync(TransitionToResultsPhaseRequest transitionToResultsPhaseRequest)
        {
            await Task.CompletedTask;
            if (!sessions.TryGetValue(transitionToResultsPhaseRequest.Session, out ISession session))
                return;

            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            var player = dataStore2.GetPlayerState(userInfo.Identifier);
            if (!player.IsModerator)
                return;

            var gamePhase = dataStore2.GetGamePhase(transitionToResultsPhaseRequest.Session);
            if (gamePhase != GamePhase.Voting)
                return;

            dataStore2.SetGamePhase(transitionToResultsPhaseRequest.Session, GamePhase.Results);
            var response = TransitionToResultsResponse.FromPlayerStates(
                dataStore2.PlayersIn(transitionToResultsPhaseRequest.Session));
            await session.RaiseTransitionToResultsAsync(response);
        }

        /// <inheritdoc/>
        public async Task UpdateParticipantAsync(UpdateParticipantRequest updateParticipantRequest)
        {
            if (!sessions.TryGetValue(updateParticipantRequest.Session, out ISession session))
                return;

            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            var player = dataStore2.GetPlayerState(userInfo.Identifier);
            var moderator = updateParticipantRequest.IsModerator ?? player.IsModerator;

            dataStore2.SetIsModerator(userInfo.Identifier, moderator);

            await session.RaisePlayerUpdatedAsync(new PlayerUpdatedResponse()
            {
                Id = userInfo.Identifier,
                IsModerator = moderator
            });
        }

        /// <inheritdoc/>
        public async Task UpdateVoteAsync(UpdateVoteRequest updateVoteRequest)
        {
            logger.LogDebug($"Enter {nameof(UpdateVoteAsync)}");
            if (!sessions.TryGetValue(updateVoteRequest.Session, out ISession session))
                return;

            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            var player = dataStore2.GetPlayerState(userInfo.Identifier);
            var gamePhase = dataStore2.GetGamePhase(updateVoteRequest.Session);
            if (gamePhase != GamePhase.Voting)
            {
                logger.LogError($"Player '{userInfo.Identifier}' attempted to vote during a non-voting phase of play.");
                return;
            }

            logger.LogDebug($"In vote is {updateVoteRequest.Vote}.");
            if (!int.TryParse(updateVoteRequest.Vote, out int outVote) && !(
                outVote < (int)ModifiedFibonacciVote.Zero ||
                outVote > (int)ModifiedFibonacciVote.Unknown))
            {
                logger.LogError($"Player '{userInfo.Identifier}' sent an illegal vote.");
                return;
            }

            logger.LogDebug($"Outvote is {outVote}.");
            dataStore2.SetVote(userInfo.Identifier, outVote);

            await session.RaiseVoteUpdatedAsync(new VoteUpdatedResponse() { Id = userInfo.Identifier });

            logger.LogDebug($"Exit {nameof(UpdateVoteAsync)}");
        }
    }
}
