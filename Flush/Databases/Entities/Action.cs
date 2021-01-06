namespace Flush.Databases.Entities
{
    /// <summary>
    /// Actions that may be taken by a <see cref="Player"/> in a
    /// <see cref="Game"/> of Scrum Poker.
    /// </summary>
    public enum Action
    {
        /// <summary>
        /// A <see cref="Player"/> joined.
        /// </summary>
        Join,
        /// <summary>
        /// A <see cref="Player"/> voted.
        /// </summary>
        Vote,
        /// <summary>
        /// A <see cref="Player"/> readied up.
        /// </summary>
        Ready,
        /// <summary>
        /// A <see cref="Game"/> was restarted.
        /// </summary>
        Restart,
        /// <summary>
        /// A <see cref="Player"/> left.
        /// </summary>
        Leave
    }
}
