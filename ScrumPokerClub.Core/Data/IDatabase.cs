using ScrumPokerClub.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScrumPokerClub.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDatabase
    {
        // profiles
        Task<Profile> GetProfileAsync(string profileId);
        Task UpsertProfileAsync(Profile profile);

        // player (a 'per-session' profile
        Task UpsertPlayerContextAsync(string sessionId, string profileId, PlayerContext player);
        Task<PlayerContext> GetPlayerContextAsync(string sessionId, string profileId);

        // votes
        Task SetVoteAsync(string sessionId, string profileId, int? vote);
        Task<int?> GetVoteAsync(string sessionId, string profileId);

        // gamephase
        Task SetGamePhaseAsync(string sessionId, GamePhase gamePhase);
        Task<GamePhase> GetGamePhaseAsync(string sessionId);

        // join session
        Task EnsureSessionContextExistsAsync(string sessionId);
        Task AddToSessionContextAsync(string sessionId, string profileId, IUserInfoService userInfoService);

        // leave session
        Task RemoveFromSessionContextAsync(string sessionId, string profileId);

        // helpers
        Task<IEnumerable<PlayerContext>> GetAllPlayerContextsInSessionContextAsync(string sessionId);

        // sticky sessions
        Task<Session> GetSessionAsync(string sessionId);
        Task UpsertSessionAsync(Session session);
    }
}
