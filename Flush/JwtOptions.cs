using Flush.Contracts;

namespace Flush
{
    /// <summary>
    /// Models the JwtAuthentication application settings section.
    /// </summary>
    public class JwtOptions : ISecretKeyOptions
    {
        /// <inheritdoc />
        public string Key { get; set; }
    }
}
