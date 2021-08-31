using ScrumPokerClub.Data.Entities;

namespace ScrumPokerClub.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class PlayerContext
    {
        /// <summary>
        ///
        /// </summary>
        public Profile Profile { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public int? LastVote { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Moderating { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayName
        {
            get
            {
                var result = ephemeralName;
                if (!string.IsNullOrWhiteSpace(Profile.DisplayName))
                    result = Profile.DisplayName;
                return result;
            }
            set
            {
                ephemeralName = value;
            }
        }
        private string ephemeralName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        public PlayerContext(IUserInfoService userInfoService, Profile profile)
        {
            Profile = profile;
            this.ephemeralName = userInfoService.Name;
        }
    }
}
