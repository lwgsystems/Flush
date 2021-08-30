using System;

namespace ScrumPokerClub.Services.Requests
{
    /// <summary>
    /// Session configuration request context.
    /// </summary>
    class ConfigureSessionRequest
    {
        /// <summary>
        /// The session identifier.
        /// </summary>
        public string Session { get; init; }

        /// <summary>
        /// The configuration steps.
        /// </summary>
        public Action<ISession> Configure { get; init; }
    }
}
