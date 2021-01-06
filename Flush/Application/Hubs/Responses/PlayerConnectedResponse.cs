namespace Flush.Application.Hubs.Responses
{
    public class PlayerConnectedResponse
    {
        public string PlayerID { get; set; }
        public string Player { get; set; }
        public int AvatarID { get; set; }
    }
}
