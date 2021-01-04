namespace Flush.Data
{
    /// <summary>
    /// Models the Flush database options application settings
    /// section.
    /// </summary>
    public class FlushDatabaseOptions : IDatabaseOptions
    {
        /// <inheritdoc />
        public string ConnectionString { get; set; }

        /// <inheritdoc />
        public string Key { get; set; }
    }
}
