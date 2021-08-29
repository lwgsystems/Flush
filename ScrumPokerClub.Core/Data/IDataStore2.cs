using System;
using System.Collections.Generic;

namespace ScrumPokerClub.Data
{
    public interface IDataStore2 : IDataStore
    {
        void SetConnectionState(string player, bool state);
        bool GetConnectionState(string player);
        IEnumerable<KeyValuePair<string, DateTime>> GetDisconnectedPlayers();
        void SetIsModerator(string player, bool isModerator);
    }
}
