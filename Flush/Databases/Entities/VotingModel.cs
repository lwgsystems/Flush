using System.ComponentModel;

namespace Flush.Databases.Entities
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
        [Description(nameof(ModifiedFibonacciVote))]
        ModifiedFibonacci,

        /// <summary>
        /// <see cref="TShirtSizeVote"/>
        /// </summary>
        [Description(nameof(TShirtSizeVote))]
        TShirtSize,
    }
}
