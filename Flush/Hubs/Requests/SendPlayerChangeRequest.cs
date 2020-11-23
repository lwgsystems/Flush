namespace Flush.Hubs.Requests
{
    public class SendPlayerChangeRequest
    {
        public bool? Observer { get; set; }
        public bool? Moderator { get; set; }
    }
}
