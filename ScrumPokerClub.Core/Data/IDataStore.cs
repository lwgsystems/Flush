using System.Collections.Generic;

namespace ScrumPokerClub.Data
{
    public interface IDataStore
    {
        void AddPlayer(string player, string name, string group);
        bool AnyPlayersIn(string game);
        GamePhase GetGamePhase(string game);
        PlayerState GetPlayerState(string player);
        IEnumerable<PlayerState> PlayersIn(string game);
        void RemovePlayer(string player);
        void SetGamePhase(string game, GamePhase phase);
        void SetVote(string player, int? vote);
    }
}
