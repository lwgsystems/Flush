using Flush.Utils;

namespace Flush.Data
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
