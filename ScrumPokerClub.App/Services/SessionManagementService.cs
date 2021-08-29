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
    /// 
    /// </summary>
    class SessionManagementService : ISessionManagementService
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IDictionary<string, ISession> sessions;

        /// <summary>
        /// 
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// 
        /// </summary>
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        private readonly IDataStore2 dataStore2;

        /// <summary>
        /// 
        /// </summary>
        public SessionManagementService(
            ILogger<SessionManagementService> logger,
            IServiceProvider serviceProvider,
            IDataStore2 dataStore2)
        {
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

            configureSessionRequest?.Configure(session);

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

            // TODO construct game state and provide downstream to newly connected player.
            // May be the case that player manually requests this in Blazor-land.

            var seed = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var r = new Random(seed);
            await session.RaisePlayerConnectedAsync(new PlayerConnectedResponse()
            {
                Id = userInfo.Identifier,
                Name = userInfo.Name,
                AvatarId = r.Next(1, 20)
            });

            sessions[joinSessionRequest.Session] = session;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionStateRequest"></param>
        /// <returns></returns>
        public Task<SessionStateResponse> GetSessionStateAsync(SessionStateRequest sessionStateRequest)
        {
            // todo get the session state
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task LeaveSessionAsync(LeaveSessionRequest leaveSessionRequest)
        {
            await Task.CompletedTask;
            if (!sessions.TryGetValue(leaveSessionRequest.Session, out ISession session))
                return;

            leaveSessionRequest.PredisconnectAction?.Invoke(session);
            session.Decrement();

            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            dataStore2.SetConnectionState(userInfo.Identifier, false);

            await session.RaisePlayerDisconnectedAsync(new PlayerDisconnectedResponse()
            {
                Id = userInfo.Identifier,
            });
        }

        /// <inheritdoc/>
        public async Task TransitionToPlayPhaseAsync(TransitionToPlayPhaseRequest transitionToPlayPhaseRequest)
        {
            await Task.CompletedTask;
            if (!sessions.TryGetValue(transitionToPlayPhaseRequest.Session, out ISession session))
                return;

            // todo check player can transition

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

            // todo check player can transition

            var gamePhase = dataStore2.GetGamePhase(transitionToResultsPhaseRequest.Session);
            if (gamePhase != GamePhase.Voting)
                return;

            dataStore2.SetGamePhase(transitionToResultsPhaseRequest.Session, GamePhase.Results);
            await session.RaiseTransitionToResultsAsync(new TransitionToResultsResponse()
            {
                Votes = dataStore2
                    .PlayersIn(transitionToResultsPhaseRequest.Session)
                    // todo where p is not observer
                    .Where(ps => ps.Vote.HasValue)
                    .Select(ps => new KeyValuePair<string,int>(ps.PlayerId, ps.Vote.Value))
            });
        }

        /// <inheritdoc/>
        public async Task UpdateParticipantAsync(UpdateParticipantRequest updateParticipantRequest)
        {
            if (!sessions.TryGetValue(updateParticipantRequest.Session, out ISession session))
                return;

            using var scope = serviceProvider.CreateScope();
            var userInfo = scope.ServiceProvider.GetRequiredService<IUserInfoService>();

            var player = dataStore2.GetPlayerState(userInfo.Identifier);
            var observer = player.IsObserver; // todo set observer according to update
            var moderator = player.IsModerator; // todo set moderator according to update;

            dataStore2.SetIsObserver(userInfo.Identifier, observer);
            dataStore2.SetIsModerator(userInfo.Identifier, moderator);

            await session.RaisePlayerUpdatedAsync(new PlayerUpdatedResponse()
            {
                // todo id, changes
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

            if (player.IsObserver)
            {
                logger.LogInformation($"Player '{userInfo.Identifier}' attempted to vote, but is an observer." +
                    $"Their vote has been recorded, but will be ignored when delivering results.");
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
