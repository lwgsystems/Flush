using System;

namespace ScrumPokerClub.Services.Requests
{
    class LeaveSessionRequest
    {
        public string Session { get; init; }
        public Action<ISession> PredisconnectAction { get; init; }
    }
}
