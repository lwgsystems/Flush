using System;
using System.Collections.Generic;
using System.Linq;
using Flush.Data.Game.Model;
using Microsoft.Extensions.Logging;

namespace Flush.Data.Game.EfCore
{
    /// <summary>
    /// A data store for the Flush game, backed by an EF Core context.
    /// </summary>
    public class FlushDataStore : IDataStore
    {
        private readonly ILogger<FlushDataStore> _logger;
        private FlushContext _context;

        /// <summary>
        /// Creates a new instance of the Flush Daya Store.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        public FlushDataStore(ILogger<FlushDataStore> logger, FlushContext context)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public Model.Game CreateGame(string gameId)
        {
            var game = new Model.Game
            {
                GameId = gameId,
                Phase = GamePhase.Created,
            };
            _context.Games.Add(game);
            _context.SaveChanges();
            return game;
        }

        /// <inheritdoc />
        public bool GameExists(string gameId)
        {
            return _context.Games.SingleOrDefault(g => g.GameId == gameId) != default(Model.Game);
        }

        /// <inheritdoc />
        public IEnumerable<AuditLog> GetAllAuditLogs()
        {
            return _context.AuditLogs;
        }

        /// <inheritdoc />
        public AuditLog GetAuditLog(int auditLogId)
        {
            return _context.AuditLogs.Single(al => al.AuditLogId == auditLogId);
        }

        /// <inheritdoc />
        public IEnumerable<AuditLog> GetAuditLogsForGame(string gameId)
        {
            return _context.AuditLogs.Where(al => al.GameId == gameId);
        }

        /// <inheritdoc />
        public IEnumerable<AuditLog> GetAuditLogsForPlayer(string playerId)
        {
            return _context.AuditLogs.Where(al => al.PlayerId == playerId);
        }

        /// <inheritdoc />
        public Model.Game GetGame(string gameId)
        {
            return _context.Games.Single(g => g.GameId == gameId);
        }

        /// <inheritdoc />
        public Model.Game GetGameForPlayer(string playerId)
        {
            Model.Game game = null;
            try
            {
                var player = GetPlayer(playerId);
                game = GetGame(player.GameId);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, string.Empty);
            }
            return game;
        }

        /// <inheritdoc />
        public Player GetPlayer(string playerId)
        {
            Player player = null;
            try
            {
                player = _context.Players.Single(p => p.PlayerId == playerId);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, string.Empty);
            }
            return player;
        }

        /// <inheritdoc />
        public IEnumerable<Player> GetPlayersInGame(string gameId)
        {
            return GetGame(gameId).Players;
        }

        /// <inheritdoc />
        public Player JoinGame(string gameId, string playerId, string playerName)
        {
            var game = _context.Games.Single(g => g.GameId == gameId);
            var player = new Player
            {
                PlayerId = playerId,
                Name = playerName,
                GameId = game.GameId,
                Game = game,
                Vote = null,
            };
            _context.Players.Add(player);
            _context.SaveChanges();
            return player;
        }

        /// <inheritdoc />
        public void LeaveGame(string playerId)
        {
            var player = GetPlayer(playerId);
            // TODO: Check this cascades and removes the game link.
            _context.Players.Remove(player);
            _context.SaveChanges();
        }

        /// <inheritdoc />
        public void ResetGame(string gameId)
        {
            var game = _context.Games.Single(g => g.GameId == gameId);
            game.Phase = GamePhase.Created;
            foreach (var player in game.Players)
            {
                player.Vote = null;
            }
            _context.SaveChanges();
        }

        /// <inheritdoc />
        public void SetGameOwner(string gameId, string playerId)
        {
            throw new NotSupportedException("Ownership/Moderation not supported.");
        }

        /// <inheritdoc />
        public void SetGamePhase(string gameId, GamePhase gamePhase)
        {
            var game = GetGame(gameId);
            game.Phase = gamePhase;
            _context.SaveChanges();
        }

        /// <inheritdoc />
        public Player SetObserver(string playerId, bool isObserver)
        {
            var player = GetPlayer(playerId);
            player.IsObserver = isObserver;
            _context.SaveChanges();
            return player;
        }

        /// <inheritdoc />
        public void Vote(string playerId, int vote)
        {
            var player = GetPlayer(playerId);
            player.Vote = vote;
            _context.SaveChanges();
        }

        /// <inheritdoc />
        public Player GetPlayerByPrincipal(string aspNetIdentity)
        {
            var player = _context.Players
                .SingleOrDefault(p => p.AspNetIdentity == aspNetIdentity);
            return player;
        }

        /// <inheritdoc />
        public Player SetPlayerId(string playerId, string newId)
        {
            var player = _context.Players
                .SingleOrDefault(p => p.PlayerId == playerId);
            if (player != null)
            {
                player.PlayerId = newId;
                _context.SaveChanges();
            }
            return player;
        }

        /// <inheritdoc />
        public Player SetPlayerPrincipal(string playerId, string aspNetIdentity)
        {
            var player = _context.Players
                .SingleOrDefault(p => p.PlayerId == playerId);
            if (player != null)
            {
                player.AspNetIdentity = aspNetIdentity;
                _context.SaveChanges();
            }
            return player;
        }
    }
}
