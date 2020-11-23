using System;
using System.Collections.Generic;
using System.Linq;
using Flush.Data.Game.Model;

namespace Flush.Data.Game.InMemory
{
    /// <summary>
    /// A very small in memory datastore that scaffolds the remaining parameters
    /// needed by the game to support the SignalR User/Group paradigm.
    /// </summary>
    public class InMemoryDataStore : IDataStore2
    {
        private static readonly Random random = new Random();

        private Dictionary<string, PlayerState> _playerState =
            new Dictionary<string, PlayerState>();

        private Dictionary<string, GamePhase> _gamePhase =
            new Dictionary<string, GamePhase>();

        /// <summary>
        /// Add a new player to a game.
        /// </summary>
        /// <param name="player">The player identifier.</param>
        /// <param name="name">The player name.</param>
        /// <param name="group">The game name.</param>
        public void AddPlayer(string player, string name, string group)
        {
            if (!_playerState.ContainsKey(player))
                _playerState.Add(player, new PlayerState()
                {
                    Name = name,
                    PlayerId = player,
                    AvatarId = random.Next(1, 20),
                    Group = group
                });
        }

        /// <summary>
        /// Remove a player from a game.
        /// </summary>
        /// <param name="player">The player.</param>
        public void RemovePlayer(string player)
        {
            if (_playerState.ContainsKey(player))
                _playerState.Remove(player);
        }

        /// <summary>
        /// Set the phase of the game.
        /// </summary>
        /// <param name="game">The game name.</param>
        /// <param name="phase">The game phase.</param>
        public void SetGamePhase(string game, GamePhase phase)
        {
            if (!_gamePhase.ContainsKey(game))
                _gamePhase.Add(game, phase);
            else
                _gamePhase[game] = phase;
        }

        /// <summary>
        /// Get the phase of the game.
        /// </summary>
        /// <param name="game">The game name.</param>
        /// <returns>The game phase.</returns>
        public GamePhase GetGamePhase(string game)
        {
            _gamePhase.TryGetValue(game, out GamePhase phase);
            return phase;
        }

        /// <summary>
        /// Gets the state of the player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>The player state.</returns>
        public PlayerState GetPlayerState(string player)
        {
            _playerState.TryGetValue(player, out PlayerState state);
            return state;
        }

        /// <summary>
        /// Sets the players observer state.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="isObserver">
        /// A value indicating their observer status.
        /// </param>
        public void SetIsObserver(string player, bool isObserver)
        {
            _playerState[player].IsObserver = isObserver;
        }

        /// <summary>
        /// Sets the players moderator state.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="isModerator">
        /// A value indicating their moderator status.
        /// </param>
        public void SetIsModerator(string player, bool isModerator)
        {
            _playerState[player].IsModerator = isModerator;
        }

        /// <summary>
        /// Sets the players vote.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="vote">The vote.</param>
        public void SetVote(string player, int? vote)
        {
            _playerState[player].Vote = vote;
        }

        /// <summary>
        /// Get a boolean value indicating if a game has players.
        /// </summary>
        /// <param name="game">The game name.</param>
        /// <returns>True if any players are in game, else false.</returns>
        public bool AnyPlayersIn(string game)
        {
            return _playerState.Values.Any(ps => ps.Group == game);
        }

        /// <summary>
        /// Get all players in a game.
        /// </summary>
        /// <param name="game">The game name.</param>
        /// <returns>
        /// An enumerable of player states associated with the game.
        /// </returns>
        public IEnumerable<PlayerState> PlayersIn(string game)
        {
            return _playerState.Values.Where(ps => ps.Group == game);
        }

        /// <summary>
        /// Sets the connection state of the player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="state">True if connected, else false.</param>
        public void SetConnectionState(string player, bool state)
        {
            _playerState[player].LastSeen = state ? null : DateTime.Now;
        }

        /// <summary>
        /// Gets the connection state of the player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>True if they are connected, else false.</returns>
        public bool GetConnectionState(string player)
        {
            return !_playerState[player].LastSeen.HasValue;
        }

        public IEnumerable<KeyValuePair<string, DateTime>> GetDisconnectedPlayers()
        {
            return _playerState.Values
                .Where(ps => ps.LastSeen.HasValue)
                .Select(ps => KeyValuePair.Create(ps.PlayerId, ps.LastSeen.Value));
        }
    }
}
