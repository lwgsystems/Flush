﻿using System.Collections.Generic;
using Flush.Data.Game.Model;

namespace Flush.Data.Game
{
    /// <summary>
    /// Describes the interface of a data store for Flush.
    /// </summary>
    public interface IDataStore
    {
        /// <summary>
        /// Gets a boolean indicating existence of a game.
        /// </summary>
        /// <param name="gameId">The game id.</param>
        /// <returns>True if the game exists, else false.</returns>
        bool GameExists(string gameId);

        /// <summary>
        /// Create a game.
        /// </summary>
        /// <param name="gameId">The game id.</param>
        /// <returns>The game.</returns>
        Model.Game CreateGame(string gameId);

        /// <summary>
        /// Reset a game.
        /// </summary>
        /// <param name="gameId">The game id.</param>
        void ResetGame(string gameId);

        /// <summary>
        /// Get an existing game.
        /// </summary>
        /// <param name="gameId">The game id.</param>
        /// <returns>The game.</returns>
        Model.Game GetGame(string gameId);

        /// <summary>
        /// Get the game that the specified player is in.
        /// </summary>
        /// <param name="playerId">The player id.</param>
        /// <returns>The game, if the player is in one, else null.</returns>
        Model.Game GetGameForPlayer(string playerId);

        /// <summary>
        /// Set the owner of a game.
        /// </summary>
        /// <param name="gameId">The game.</param>
        /// <param name="playerId">The owning player.</param>
        void SetGameOwner(string gameId, string playerId);

        /// <summary>
        /// Set the phase of a game.
        /// </summary>
        /// <param name="gameId">The game.</param>
        /// <param name="gamePhase">The phase.</param>
        void SetGamePhase(string gameId, GamePhase gamePhase);

        /// <summary>
        /// Gets an enumerable of all players in game.
        /// </summary>
        /// <param name="gameId">The game.</param>
        /// <returns>An enumerable of all players in the game.s</returns>
        IEnumerable<Player> GetPlayersInGame(string gameId);

        /// <summary>
        /// Get the player.
        /// </summary>
        /// <param name="playerId">The player id.</param>
        /// <returns>The player.</returns>
        Player GetPlayer(string playerId);

        /// <summary>
        /// Get a player by their ASP.NET Identity.
        /// </summary>
        /// <param name="aspNetIdentity">The asp.net identity.</param>
        /// <returns>The player.</returns>
        Player GetPlayerByPrincipal(string aspNetIdentity);

        /// <summary>
        /// Set a players id.
        /// </summary>
        /// <param name="playerId">The player id.</param>
        /// <param name="newId">The new player id.</param>
        /// <returns>The player.</returns>
        Player SetPlayerId(string playerId, string newId);

        /// <summary>
        /// Set a players principal.
        /// </summary>
        /// <param name="playerId">The player id.</param>
        /// <param name="aspNetIdentity">The new principal.</param>
        /// <returns>The player.</returns>
        Player SetPlayerPrincipal(string playerId, string aspNetIdentity);

        /// <summary>
        /// Set a players observer status.
        /// </summary>
        /// <param name="playerId">The player id.</param>
        /// <param name="isObserver">The players new observer status.</param>
        /// <returns>The player.</returns>
        Player SetObserver(string playerId, bool isObserver);

        /// <summary>
        /// Join a game.
        /// </summary>
        /// <param name="gameId">The game id.</param>
        /// <param name="playerId">The player id.</param>
        /// <param name="playerName">The player name.</param>
        /// <returns>The player.</returns>
        Player JoinGame(string gameId, string playerId, string playerName);

        /// <summary>
        /// Leave a game.
        /// </summary>
        /// <param name="playerId">The player id.</param>
        void LeaveGame(string playerId);

        /// <summary>
        /// Register a vote.
        /// </summary>
        /// <param name="playerId">The player.</param>
        /// <param name="vote">The vote.</param>
        void Vote(string playerId, int vote);

        /// <summary>
        /// Get an item from the audit log.
        /// </summary>
        /// <param name="auditLogId">The log id.</param>
        /// <returns>The log item.</returns>
        AuditLog GetAuditLog(int auditLogId);

        /// <summary>
        /// Get all logs generated by a player.
        /// </summary>
        /// <param name="playerId">The player id.</param>
        /// <returns>
        /// An enumerable containing all logs generated by a player.
        /// </returns>
        IEnumerable<AuditLog> GetAuditLogsForPlayer(string playerId);

        /// <summary>
        /// Gets all logs generated in a game.
        /// </summary>
        /// <param name="gameId">The game id.</param>
        /// <returns>
        /// An enumerable containing all logs generated in a game.
        /// </returns>
        IEnumerable<AuditLog> GetAuditLogsForGame(string gameId);

        /// <summary>
        /// Gets all logs.
        /// </summary>
        /// <returns>An enumerable containing all logs.</returns>
        IEnumerable<AuditLog> GetAllAuditLogs();
    }
}
