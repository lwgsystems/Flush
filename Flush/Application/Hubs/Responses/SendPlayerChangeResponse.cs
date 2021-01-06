namespace Flush.Application.Hubs.Responses
{
    public class SendPlayerChangeResponse
    {
        public string PlayerID { get; set; }
        public bool IsObserver { get; set; }
        public bool HasVoted { get; set; }
        public bool IsModerator { get; set; }
    }
}
