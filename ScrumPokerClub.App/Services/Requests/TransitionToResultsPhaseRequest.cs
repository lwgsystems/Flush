namespace ScrumPokerClub.Services.Requests
{
    /// <summary>
    /// Play to results transition request context.
    /// </summary>
    class TransitionToResultsPhaseRequest
    {
        /// <summary>
        /// The session identifier.
        /// </summary>
        public string Session { get; init; }
    }
}
