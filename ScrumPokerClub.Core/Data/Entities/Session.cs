namespace ScrumPokerClub.Data.Entities
{
    /// <summary>
    /// A session.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// The session id.
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The session name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The owning profile, if any.
        /// </summary>
        public string ProfileId { get; set; }

        /// <summary>
        /// The owning profile, if any.
        /// </summary>
        public Profile Profile { get; set; }
    }
}
