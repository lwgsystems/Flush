namespace Flush.Utils
{
    /// <summary>
    /// Models a secret key options applications settings section.
    /// </summary>
    public interface ISecretKeyOptions
    {
        /// <summary>
        /// The hash algorithm with which to derive a key.
        /// </summary>
        public string HashAlgorithm { get; set; }

        /// <summary>
        /// The thumbprint of the certificate to use in key derivation.
        /// </summary>
        public string Thumbprint { get; set; }
    }
}
