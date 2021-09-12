namespace ScrumPokerClub.Services.Requests
{
    /// <summary>
    /// Participant vote update request context.
    /// </summary>
    class UpdateVoteRequest
    {
        /// <summary>
        /// The session identifier.
        /// </summary>
        public string Session { get; init; }

        /// <summary>
        /// The participant vote.
        /// </summary>
        public string Vote { get; init; }
    }
}
