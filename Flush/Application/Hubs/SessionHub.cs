using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Flush.Application.Hubs.Requests;
using Flush.Application.Hubs.Responses;
using Flush.Contracts;
using Flush.Databases.Entities;
using Flush.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Flush.Application.Hubs
{
    /// <summary>
    /// Hub providing management and marshalling for scrumpoker games.
    /// </summary>
    [Authorize]
    public class SessionHub : Hub
    {
        private ILogger<SessionHub> _logger;
        private readonly IDataStore2 dataStore2;

        private string Player =>
            Context.User.FindFirst(ClaimTypes.NameIdentifier)?.GetFlushUsername();
        private string Room =>
            Context.User.FindFirst(ClaimTypes.NameIdentifier)?.GetFlushRoom();
        private string PlayerID =>
            Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        /// <summary>
        /// Constructs a new hub.
        /// </summary>
        /// <param name="playerInfo">The current IDataStore instance.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="logoutProvider">The logout provider.</param>
        public SessionHub(ILogger<SessionHub> logger,
            IDataStore2 playerInfo)
        {
            _logger = logger;
            dataStore2 = playerInfo;
        }

        /// <summary>
        /// Construct a GameStateInfo structure.
        /// </summary>
        /// <returns>A filled GameStateInfo</returns>
        private PlayerConnectedRequiresGameStateResponse CreatePlayerConnectedRequiresGameStateResponse()
        {
            _logger.LogDebug($"Enter {nameof(CreatePlayerConnectedRequiresGameStateResponse)}");

            var player = dataStore2.GetPlayerState(PlayerID);
            var playerConnectedRequiresGameStateResponse = new PlayerConnectedRequiresGameStateResponse()
            {
                Phase = dataStore2.GetGamePhase(Room),
                Players = dataStore2.PlayersIn(Room)
                    .Select(p => new PlayerConnectedRequiresGameStateResponse.PlayerData()
                    {
                        PlayerID = p.PlayerId,
                        Player = p.Name,
                        Vote = p.Vote,
                        AvatarID = p.AvatarId,
                        IsModerator = p.IsModerator,
                        IsObserver = p.IsObserver
                    })
            };

            _logger.LogDebug($"Exit {nameof(CreatePlayerConnectedRequiresGameStateResponse)}");
            return playerConnectedRequiresGameStateResponse;
        }

        /// <summary>
        /// Connection routine.
        /// </summary>
        /// <returns>A task representing the request.</returns>
        public override async Task OnConnectedAsync()
        {
            _logger.LogDebug($"Enter {nameof(OnConnectedAsync)}");
            _logger.LogDebug($"Player {Context.ConnectionId} connected as {PlayerID}.");

            await Groups.AddToGroupAsync(Context.ConnectionId, Room);

            // If we're the first to join, move to voting.
            if (!dataStore2.AnyPlayersIn(Room))
            {
                dataStore2.SetGamePhase(Room, GamePhase.Voting);
            }

            // If the player hasn't connected before, add a state container
            if (dataStore2.GetPlayerState(PlayerID) == null)
            {
                dataStore2.AddPlayer(PlayerID, Player, Room);
            } else
            {
                dataStore2.SetConnectionState(PlayerID, true);
            }

            // Construct game state and send downstream to new player.
            var playerConnectedRequiresGameStateResponse = CreatePlayerConnectedRequiresGameStateResponse();
            await Clients
                .Caller
                .SendAsync("ReceiveGameStateFromJoinRoom", playerConnectedRequiresGameStateResponse);

            // Notify everyone else
            var avatarId = dataStore2.GetPlayerState(PlayerID).AvatarId;
            var playerConnectedResponse = new PlayerConnectedResponse()
            {
                PlayerID = PlayerID,
                Player = Player,
                AvatarID = avatarId
            };

            await Clients
                .OthersInGroup(Room)
                .SendAsync("PlayerJoined", playerConnectedResponse);

            _logger.LogDebug($"Exit {nameof(OnConnectedAsync)}");
        }

        /// <summary>
        /// Disconnection routine.
        /// </summary>
        /// <param name="exception">The exception, if raised.</param>
        /// <returns>A task representing the request.</returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogDebug($"Enter {nameof(OnDisconnectedAsync)}");
            _logger.LogDebug($"Player '{PlayerID}' ('{Context.ConnectionId}') disconnected.");

            // Disconnected is a strange state. It may not be deliberate.
            // We'll treat the user as gone, and notify the clients.
            // However, there will be a delay between this and the user being purged.
            // If they reconnect, it'll be like nothing happened.
            var player = dataStore2.GetPlayerState(PlayerID);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Room);
            dataStore2.SetConnectionState(PlayerID, false);
            await Clients.Group(Room)
                .SendAsync("PlayerDisconnected", new PlayerDisconnectedResponse()
                {
                    PlayerID = PlayerID
                });

            _logger.LogDebug($"Exit {nameof(OnDisconnectedAsync)}");
        }


        /// <summary>
        /// Notify clients that a user has voted.
        /// </summary>
        /// <param name="inVote">The vote.</param>
        /// <returns>A task representing the request.</returns>
        public async Task SendVote(SendVoteRequest request)
        {
            _logger.LogDebug($"Enter {nameof(SendVote)}");

            var player = dataStore2.GetPlayerState(PlayerID);
            var gamePhase = dataStore2.GetGamePhase(Room);
            if (gamePhase != GamePhase.Voting)
            {
                _logger.LogError($"Player '{PlayerID}' attempted to vote during a non-voting phase of play.");
                return;
            }

            if (player.IsObserver)
            {
                _logger.LogInformation($"Player '{PlayerID}' attempted to vote, but is an observer." +
                    $"Their vote has been recorded, but will be ignored when delivering results.");
            }

            _logger.LogDebug($"In vote is {request.Vote}.");
            if (!int.TryParse(request.Vote, out int outVote) && !(
                outVote < (int)ModifiedFibonacciVote.Zero ||
                outVote > (int)ModifiedFibonacciVote.Unknown))
            {
                    _logger.LogError($"Player '{PlayerID}' sent an illegal vote.");
                    return;
            }

            _logger.LogDebug($"Outvote is {outVote}.");
            dataStore2.SetVote(PlayerID, outVote);
            await Clients
                .Group(Room)
                .SendAsync("PlayerVoted", new SendVoteResponse() { PlayerID = PlayerID });

            _logger.LogDebug($"Exit {nameof(SendVote)}");
        }

        /// <summary>
        /// Construct a SendResultResponse structure.
        /// </summary>
        /// <returns>A filled SendResultResponse</returns>
        private SendResultResponse CreateSendResultResponse()
        {
            _logger.LogDebug($"Enter {nameof(CreateSendResultResponse)}");

            var player = dataStore2.GetPlayerState(PlayerID);
            var sendResultResponse = new SendResultResponse()
            {
                Votes = dataStore2.PlayersIn(Room)
                    .Where(p => !p.IsObserver && p.Vote.HasValue)
                    .Select(p => new SendResultResponse.VoteInfo()
                    {
                        PlayerID = p.PlayerId,
                        Vote = p.Vote.Value
                    })
            };

            _logger.LogDebug($"Exit {nameof(CreateSendResultResponse)}");
            return sendResultResponse;
        }

        /// <summary>
        /// Inform the clients to reveal the votes.
        /// </summary>
        /// <returns>A task representing the request.</returns>
        public async Task SendResult(SendResultRequest request)
        {
            _logger.LogDebug($"Enter {nameof(SendResult)}");

            var player = dataStore2.GetPlayerState(PlayerID);
            if (!player.IsModerator)
            {
                _logger.LogError($"Player '{PlayerID}' tried to perform a moderator-only action.");
                return;
            }

            var gamePhase = dataStore2.GetGamePhase(Room);
            if (gamePhase != GamePhase.Voting)
            {
                _logger.LogError($"Player '{PlayerID}' requested vote results during a non-voting phase of play.");
                return;
            }

            var sendResultResponse = CreateSendResultResponse();
            dataStore2.SetGamePhase(Room, GamePhase.Results);
            await Clients
                .Group(Room)
                .SendAsync("StartDiscussionPhase", sendResultResponse);

            _logger.LogDebug($"Exit {nameof(SendResult)}");
        }

        /// <summary>
        /// Inform the clients to reset the vote.
        /// </summary>
        /// <returns>A task representing the request.</returns>
        public async Task BeginVoting(BeginVotingRequest request)
        {
            _logger.LogDebug($"Enter {nameof(BeginVoting)}");

            var player = dataStore2.GetPlayerState(PlayerID);
            if (!player.IsModerator)
            {
                _logger.LogError($"Player '{PlayerID}' tried to perform a moderator-only action.");
                return;
            }

            var gamePhase = dataStore2.GetGamePhase(Room);
            if (gamePhase != GamePhase.Results)
            {
                _logger.LogError($"Player '{PlayerID}' requested a return to voting during a non-results phase of play.");
                return;
            }

            // reset the votes
            var players = dataStore2.PlayersIn(Room);
            foreach (var p in players)
                dataStore2.SetVote(p.PlayerId, null);

            dataStore2.SetGamePhase(Room, GamePhase.Voting);
            await Clients
                .Group(Room)
                .SendAsync("StartVotingPhase", new BeginVotingResponse());

            _logger.LogDebug($"Exit {nameof(BeginVoting)}");
        }

        /// <summary>
        /// Notify users of a change in player state.
        /// </summary>
        /// <param name="isObserver">If this player is an observer.</param>
        /// <returns>A task representing the request.</returns>
        public async Task SendPlayerChange(SendPlayerChangeRequest request)
        {
            _logger.LogDebug($"Enter {nameof(SendPlayerChange)}");

            var player = dataStore2.GetPlayerState(PlayerID);
            var observer = request.Observer ?? player.IsObserver;
            var moderator = request.Moderator ?? player.IsModerator;

            dataStore2.SetIsObserver(PlayerID, observer);
            dataStore2.SetIsModerator(PlayerID, moderator);

            var sendPlayerChangeResponse = new SendPlayerChangeResponse()
            {
                PlayerID = PlayerID,
                IsObserver = observer,
                IsModerator = moderator
            };

            await Clients
                .Group(Room)
                .SendAsync("PlayerChanged", sendPlayerChangeResponse);

            _logger.LogDebug($"Exit {nameof(SendPlayerChange)}");
        }
    }
}
