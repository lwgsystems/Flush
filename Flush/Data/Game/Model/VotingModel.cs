namespace Flush.Data.Game.Model
{
    /// <summary>
    /// Defines the available voting models, such as 'modified fibonacci.'
    /// This helps to translate votes to human-readable names.
    /// I.e. in modified fibonacci, Vote=6 => MFV=8
    /// </summary>
    public enum VotingModel
    {
        /// <summary>
        /// <see cref="ModifiedFibonacciVote"/>
        /// </summary>
        ModifiedFibonacci,
    }
}
