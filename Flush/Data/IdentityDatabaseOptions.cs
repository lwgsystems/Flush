namespace Flush.Data
{
    /// <summary>
    /// Models the ASP.NET Core Identity database options application settings
    /// section.
    /// </summary>
    public class IdentityDatabaseOptions : IDatabaseOptions
    {
        /// <inheritdoc />
        public string ConnectionString { get; set; }

        /// <inheritdoc />
        public string HashAlgorithm { get; set; }

        /// <inheritdoc />
        public string Thumbprint { get; set; }
    }
}
