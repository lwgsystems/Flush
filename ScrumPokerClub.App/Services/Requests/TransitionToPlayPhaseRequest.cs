namespace ScrumPokerClub.Services.Requests
{
    /// <summary>
    /// Results to play transition request context.
    /// </summary>
    class TransitionToPlayPhaseRequest
    {
        /// <summary>
        /// The session identifier.
        /// </summary>
        public string Session { get; init; }
    }
}
