using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScrumPokerClub.Data;

namespace ScrumPokerClub.Services.Responses
{
    class SessionStateResponse
    {
        public GamePhase GamePhase { get; init; }
        public IEnumerable<PlayerState> Players { get; init; }
    }
}
