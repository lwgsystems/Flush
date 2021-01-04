namespace Flush.Utils
{
    /// <summary>
    /// Models a secret key options applications settings section.
    /// </summary>
    public interface ISecretKeyOptions
    {
        /// <summary>
        /// The thumbprint of the certificate to use in key derivation.
        /// </summary>
        public string Key { get; set; }
    }
}
