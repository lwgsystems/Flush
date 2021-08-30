using System;

namespace ScrumPokerClub.Data
{
    /// <summary>
    /// A subset of <see cref="Player"/> information used for tracking player
    /// state in memory.
    public class PlayerState
    {
        /// <summary>
        /// The player name.
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// The player id.
        /// </summary>
        public string PlayerId { get; set; } = null;

        /// <summary>
        /// The avatar id.
        /// </summary>
        public int AvatarId { get; set; } = 1;

        /// <summary>
        /// The game this player is in.
        /// </summary>
        public string Group { get; set; } = null;

        /// <summary>
        /// Get or set a value indicating that this player is a moderator.
        /// </summary>
        public bool IsModerator { get; set; } = false;

        /// <summary>
        /// The players most recent vote.
        /// </summary>
        public int? Vote { get; set; } = null;

        /// <summary>
        /// Get a value indicating when the player was last seen.
        /// If null, the player is active.
        /// </summary>
        public DateTime? LastSeen = null;
    }
}
