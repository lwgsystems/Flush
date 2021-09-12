using System.Collections.Generic;

namespace ScrumPokerClub.Data.Entities
{
    /// <summary>
    /// A players custom profile.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// The player id (provided by MIP.)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The avatar id.
        /// </summary>
        public int AvatarId { get; set; }

        /// <summary>
        /// The set of sessions owned by this user.
        /// </summary>
        public List<Session> Sessions { get; set; }
    }
}
