using System;

namespace ScrumPokerClub.Services.Requests
{
    class ConfigureSessionRequest
    {
        public string Session { get; init; }
        public Action<ISession> Configure { get; init; }
    }
}
