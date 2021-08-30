namespace ScrumPokerClub.Services.Requests
{
    /// <summary>
    /// Participant update request context.
    /// </summary>
    class UpdateParticipantRequest
    {
        /// <summary>
        /// The session identifier.
        /// </summary>
        public string Session { get; init; }

        /// <summary>
        /// True if the player is requesting moderator permission, else false. Null if unchanged.
        /// </summary>
        public bool? IsModerator { get; init; }

        /// <summary>
        /// True if the player is requesting observer status, else false. Null if unchanged.
        /// </summary>
        public bool? IsObserver { get; init; }
    }
}
