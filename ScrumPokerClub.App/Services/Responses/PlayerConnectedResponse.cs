namespace ScrumPokerClub.Services.Responses
{
    /// <summary>
    /// Player connection event response context.
    /// </summary>
    class PlayerConnectedResponse
    {
        /// <summary>
        /// The player id.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// The player name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// The server assigned avatar id.
        /// </summary>
        public int AvatarId { get; init; }
    }
}
