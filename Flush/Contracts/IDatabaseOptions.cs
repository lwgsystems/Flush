namespace Flush.Contracts
{
    /// <summary>
    /// Models a database options application settings section.
    /// </summary>
    public interface IDatabaseOptions : ISecretKeyOptions
    {
        /// <summary>
        /// The connection string.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
