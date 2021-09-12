namespace ScrumPokerClub.Services.Responses
{
    /// <summary>
    /// Player updated event response context.
    /// </summary>
    class PlayerUpdatedResponse
    {
        /// <summary>
        /// The player id.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// True if the player is now a moderator, else false.
        /// </summary>
        public bool IsModerator { get; init; }

        /// <summary>
        /// The avatar id.
        /// </summary>
        public int AvatarId { get; init; }

        /// <summary>
        /// The display name.
        /// </summary>
        public string DisplayName { get; init; }
    }
}
