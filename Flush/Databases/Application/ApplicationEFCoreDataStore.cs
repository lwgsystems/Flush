using System;
using System.Collections.Generic;
using Flush.Contracts;
using Flush.Databases.Entities;
using Microsoft.Extensions.Logging;

namespace Flush.Databases.Application
{
    /// <summary>
    /// A data store for the Flush game, backed by an EF Core context.
    /// </summary>
    public class ApplicationEFCoreDataStore : IDataStore2
    {
        private readonly ILogger<ApplicationEFCoreDataStore> _logger;
        private ApplicationContext _context;

        /// <summary>
        /// Creates a new instance of the Flush Daya Store.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        public ApplicationEFCoreDataStore(ILogger<ApplicationEFCoreDataStore> logger, ApplicationContext context)
        {
            _context = context;
            _logger = logger;
        }

        public void AddPlayer(string player, string name, string group)
        {
            throw new NotImplementedException();
        }

        public bool AnyPlayersIn(string game)
        {
            throw new NotImplementedException();
        }

        public bool GetConnectionState(string player)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<string, DateTime>> GetDisconnectedPlayers()
        {
            throw new NotImplementedException();
        }

        public GamePhase GetGamePhase(string game)
        {
            throw new NotImplementedException();
        }

        public PlayerState GetPlayerState(string player)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PlayerState> PlayersIn(string game)
        {
            throw new NotImplementedException();
        }

        public void RemovePlayer(string player)
        {
            throw new NotImplementedException();
        }

        public void SetConnectionState(string player, bool state)
        {
            throw new NotImplementedException();
        }

        public void SetGamePhase(string game, GamePhase phase)
        {
            throw new NotImplementedException();
        }

        public void SetIsModerator(string player, bool isModerator)
        {
            throw new NotImplementedException();
        }

        public void SetIsObserver(string player, bool isObserver)
        {
            throw new NotImplementedException();
        }

        public void SetVote(string player, int? vote)
        {
            throw new NotImplementedException();
        }
    }
}
