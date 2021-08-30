namespace ScrumPokerClub.Services.Requests
{
    /// <summary>
    /// Session leave request context.
    /// </summary>
    class LeaveSessionRequest
    {
        /// <summary>
        /// The session identifier.
        /// </summary>
        public string Session { get; init; }

        /// <summary>
        /// The user that is leaving.
        /// </summary>
        public string Id { get; init; }
    }
}
