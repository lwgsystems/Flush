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
        /// A non-zero value if the avatar is changed. Null if unchanged.
        /// </summary>
        public int? AvatarId { get; init; }

        /// <summary>
        /// The display name, if changed. Will update profile in database.
        /// </summary>
        public string DisplayName { get; init; }
    }
}
