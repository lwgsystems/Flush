using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Flush.Data.Game.InMemory;
using Flush.Data.Game.Model;
using Flush.Extensions;
using Flush.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Flush.Hubs
{
    /// <summary>
    /// Hub providing management and marshalling for scrumpoker games.
    /// </summary>
    [Authorize]
    public class PokerGameHub : Hub
    {
        private ILogger<PokerGameHub> _logger;
        private InMemoryDataStore _inMemoryDataStore;
        private AutomaticLogoutProvider _automaticLogoutProvider;

        /// <summary>
        /// Constructs a new hub.
        /// </summary>
        /// <param name="playerInfo">The current IDataStore instance.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="logoutProvider">The logout provider.</param>
        public PokerGameHub(ILogger<PokerGameHub> logger,
            InMemoryDataStore playerInfo,
            AutomaticLogoutProvider logoutProvider)
        {
            _logger = logger;
            _inMemoryDataStore = playerInfo;
            _automaticLogoutProvider = logoutProvider;
        }

        /// <summary>
        /// Connection routine.
        /// </summary>
        /// <returns>A task representing the request.</returns>
        public override async Task OnConnectedAsync()
        {
            _logger.LogDebug($"Enter {nameof(OnConnectedAsync)}");
            _logger.LogDebug($"Player '{Context.UserIdentifier}' ('{Context.ConnectionId}') connected.");

            // cancel the logout, if one exists.
            _automaticLogoutProvider.Cancel(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var claim = Context.User.FindFirst(ClaimTypes.NameIdentifier);
            var user = claim.GetFlushUsername();
            var room = claim.GetFlushRoom();

            await Groups.AddToGroupAsync(Context.ConnectionId, room);

            // If we're the first to join, move to voting.
            if (!_inMemoryDataStore.AnyPlayersIn(room))
            {
                _inMemoryDataStore.SetGamePhase(room, GamePhase.Voting);
            }

            // If the player hasn't connected before, add a state container
            if (_inMemoryDataStore.GetPlayerState(Context.UserIdentifier) == null)
            {
                _inMemoryDataStore.AddPlayer(Context.UserIdentifier, user, room);
            }

            // Construct game state and send downstream to new player.
            var gameStateInfo = ConstructGameStateInfo();
            await Clients
                .Caller
                .SendAsync("ReceiveGameStateFromJoinRoom", gameStateInfo);

            // Notify everyone else
            var playerJoinInfo = new
            {
                PlayerId = Context.UserIdentifier,
                user
            };
            await Clients
                .OthersInGroup(room)
                .SendAsync("PlayerJoined", playerJoinInfo);

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

            _logger.LogDebug($"Player '{Context.UserIdentifier}' ('{Context.ConnectionId}') disconnected.");

            // Disconnected is a strange state. It may not be deliberate.
            // We'll remove this connection id from the group but, for now,
            // we won't notify any of the users of the disconnection, nor will
            // we treat the user as disconnected.
            var player = _inMemoryDataStore.GetPlayerState(Context.UserIdentifier);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, player.Group);

            // We'll use this opportunity to schedule their logout.
            _automaticLogoutProvider.Add(
                Context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            _logger.LogDebug($"Exit {nameof(OnDisconnectedAsync)}");
        }

        /// <summary>
        /// Construct a GameStateInfo structure.
        /// </summary>
        /// <returns>A filled GameStateInfo</returns>
        private object ConstructGameStateInfo()
        {
            _logger.LogDebug($"Enter {nameof(ConstructGameStateInfo)}");

            var player = _inMemoryDataStore.GetPlayerState(Context.UserIdentifier);
            var gameStateInfo = new
            {
                Phase = _inMemoryDataStore.GetGamePhase(player.Group),
                Players = _inMemoryDataStore.PlayersIn(player.Group)
                    .Select(p => new
                    {
                        p.PlayerId,
                        p.Name,
                        p.Vote
                    })
            };

            _logger.LogDebug($"Exit {nameof(ConstructGameStateInfo)}");
            return gameStateInfo;
        }

        /// <summary>
        /// Construct a GameResultInfo structure.
        /// </summary>
        /// <returns>A filled GameResultInfo</returns>
        private object ConstructGameResultInfo()
        {
            _logger.LogDebug($"Enter {nameof(ConstructGameResultInfo)}");

            var player = _inMemoryDataStore.GetPlayerState(Context.UserIdentifier);
            var gameResultInfo = new
            {
                Votes = _inMemoryDataStore.PlayersIn(player.Group)
                    .Where(p => !p.IsObserver)
                    .Select(p => new
                    {
                        p.PlayerId,
                        p.Vote
                    })
            };

            _logger.LogDebug($"Exit {nameof(ConstructGameResultInfo)}");
            return gameResultInfo;
        }

        /// <summary>
        /// Leave a room.
        /// </summary>
        /// <param name="user">The user who is leaving.</param>
        /// <returns>A task representing the request.</returns>
        /// <remarks>
        /// This is called by the auto-logout provider
        /// </remarks>
        public async Task LeaveRoom(string user)
        {
            _logger.LogDebug($"Enter {nameof(LeaveRoom)}");

            // remove player from the group and store
            // the player state object lives so long as we have a handle to it
            // here.
            var player = _inMemoryDataStore.GetPlayerState(user);
            _inMemoryDataStore.RemovePlayer(user);

            // If everyone has left, change to finished state.
            if (!_inMemoryDataStore.AnyPlayersIn(player.Group))
            {
                _logger.LogDebug($"All players have left {player.Group}, transitioning to finished.");
                _inMemoryDataStore.SetGamePhase(player.Group, GamePhase.Finished);
                return;
            }

            await Clients
                .Group(player.Group)
                .SendAsync("PlayerLeft", user);

            _logger.LogDebug($"Exit {nameof(LeaveRoom)}");
        }

        /// <summary>
        /// Notify clients that a user has voted.
        /// </summary>
        /// <param name="inVote">The vote.</param>
        /// <returns>A task representing the request.</returns>
        public async Task SendVote(string inVote)
        {
            _logger.LogDebug($"Enter {nameof(SendVote)}");

            var player = _inMemoryDataStore.GetPlayerState(Context.UserIdentifier);
            var gamePhase = _inMemoryDataStore.GetGamePhase(player.Group);
            if (gamePhase != GamePhase.Voting)
            {
                _logger.LogError($"Player '{Context.UserIdentifier}' attempted to vote during a non-voting phase of play.");
                return;
            }

            if (player.IsObserver)
            {
                _logger.LogInformation($"Player '{Context.UserIdentifier}' attempted to vote, but is an observer." +
                    $"Their vote has been recorded, but will be ignored when delivering results.");
            }

            if (int.TryParse(inVote, out int outVote))
            {
                if (outVote < (int)ModifiedFibonacciVote.Zero
                   || outVote > (int)ModifiedFibonacciVote.Unknown)
                {
                    _logger.LogError($"Player '{Context.UserIdentifier}' sent an illegal vote.");
                    return;
                }

                _inMemoryDataStore.SetVote(Context.UserIdentifier, outVote);
                await Clients
                    .Group(player.Group)
                    .SendAsync("PlayerVoted", Context.UserIdentifier);
            }

            _logger.LogDebug($"Exit {nameof(SendVote)}");
        }

        /// <summary>
        /// Inform the clients to reveal the votes.
        /// </summary>
        /// <returns>A task representing the request.</returns>
        public async Task SendResult()
        {
            _logger.LogDebug($"Enter {nameof(SendResult)}");

            var player = _inMemoryDataStore.GetPlayerState(Context.UserIdentifier);
            var gamePhase = _inMemoryDataStore.GetGamePhase(player.Group);
            if (gamePhase != GamePhase.Voting)
            {
                _logger.LogError($"Player '{Context.UserIdentifier}' requested vote results during a non-voting phase of play.");
                return;
            }

            var gameResultInfo = ConstructGameResultInfo();
            _inMemoryDataStore.SetGamePhase(player.Group, GamePhase.Results);
            await Clients
                .Group(player.Group)
                .SendAsync("StartDiscussionPhase", gameResultInfo);

            _logger.LogDebug($"Exit {nameof(SendResult)}");
        }

        /// <summary>
        /// Inform the clients to reset the vote.
        /// </summary>
        /// <returns>A task representing the request.</returns>
        public async Task BeginVoting()
        {
            _logger.LogDebug($"Enter {nameof(BeginVoting)}");

            var player = _inMemoryDataStore.GetPlayerState(Context.UserIdentifier);
            var gamePhase = _inMemoryDataStore.GetGamePhase(player.Group);
            if (gamePhase != GamePhase.Results)
            {
                _logger.LogError($"Player '{Context.UserIdentifier}' requested vote results during a non-results phase of play.");
                return;
            }

            // reset the votes
            // TODO: a more elegant way of achieving this (just a 'reset' will
            // do, tbh.)
            var players = _inMemoryDataStore.PlayersIn(player.Group);
            foreach (var p in players)
                _inMemoryDataStore.SetVote(p.PlayerId, null);

            _inMemoryDataStore.SetGamePhase(player.Group, GamePhase.Voting);
            await Clients
                .Group(player.Group)
                .SendAsync("StartVotingPhase");

            _logger.LogDebug($"Exit {nameof(BeginVoting)}");
        }

        /// <summary>
        /// Notify users of a change in player state.
        /// </summary>
        /// <param name="isObserver">If this player is an observer.</param>
        /// <returns>A task representing the request.</returns>
        public async Task SendPlayerChange(bool isObserver)
        {
            _logger.LogDebug($"Enter {nameof(SendPlayerChange)}");

            _inMemoryDataStore.SetIsObserver(Context.UserIdentifier, isObserver);
            var player = _inMemoryDataStore.GetPlayerState(Context.UserIdentifier);
            var playerChangeInfo = new
            {
                PlayerId = Context.UserIdentifier,
                player.IsObserver,
                HasVoted = player.Ready
            };

            await Clients
                .Group(player.Group)
                .SendAsync("PlayerChanged", playerChangeInfo);

            _logger.LogDebug($"Exit {nameof(SendPlayerChange)}");
        }
    }
}
