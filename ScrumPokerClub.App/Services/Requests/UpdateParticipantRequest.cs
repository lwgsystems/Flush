namespace ScrumPokerClub.Services.Requests
{
    class UpdateParticipantRequest
    {
        public string Session { get; init; }
        public bool? IsModerator { get; init; }
        public bool? IsObserver { get; init; }
    }
}
