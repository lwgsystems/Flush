namespace Flush.Utils
{
    /// <summary>
    /// Models the JwtAuthentication application settings section.
    /// </summary>
    public class JwtOptions : ISecretKeyOptions
    {
        /// <inheritdoc />
        public string HashAlgorithm { get; set; }

        /// <inheritdoc />
        public string Thumbprint { get; set; }
    }
}
