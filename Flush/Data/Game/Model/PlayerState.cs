namespace Flush.Data.Game.Model
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
        /// The game this player is in.
        /// </summary>
        public string Group { get; set; } = null;

        /// <summary>
        /// Get or set a value indicating that this player is an observer.
        /// </summary>
        public bool IsObserver { get; set; } = false;

        /// <summary>
        /// The players most recent vote.
        /// </summary>
        public int? Vote { get; set; } = null;

        /// <summary>
        /// Get a value indicating that the player has voted.
        /// </summary>
        public bool Ready => Vote != null;
    }
}
