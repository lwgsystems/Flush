using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScrumPokerClub.Services.Responses
{
    class PlayerUpdatedResponse
    {
        public string Id { get; init; }
        public bool IsModerator { get; init; }
        public bool IsObserver { get; init; }
    }
}
