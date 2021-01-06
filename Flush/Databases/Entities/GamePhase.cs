namespace Flush.Databases.Entities
{
    /// <summary>
    /// Models the phase of a <see cref="Game"/>
    /// </summary>
    /// <remarks>
    /// The enumeration is cyclic and order by transition.
    /// </remarks>
    public enum GamePhase
    {
        /// <summary>
        /// The <see cref="Game"/> has been created.
        /// This is only ever set once. All games subsequently transition from
        /// 'finished' to 'voting.'
        /// </summary>
        Created,

        /// <summary>
        /// The <see cref="Game"/> is in the voting phase.
        /// </summary>
        Voting,

        /// <summary>
        /// The <see cref="Game"/> is in the results phase.
        /// </summary>
        Results,

        /// <summary>
        /// The <see cref="Game"/> is finished.
        /// </summary>
        Finished
    }
}
