using ScrumPokerClub.Data;
using System.Collections.Generic;

namespace ScrumPokerClub.Services.Responses
{
    /// <summary>
    /// Session state event response context.
    /// </summary>
    class SessionStateResponse
    {
        /// <summary>
        /// The current game phase.
        /// </summary>
        public GamePhase GamePhase { get; init; }

        /// <summary>
        /// The states of all connected players.
        /// </summary>
        public IEnumerable<PlayerState> Players { get; init; }
    }
}
