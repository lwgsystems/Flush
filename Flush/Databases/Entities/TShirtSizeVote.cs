using System.ComponentModel;

namespace Flush.Databases.Entities
{
    /// <summary>
    /// Models the t-shirt size voting scheme.
    /// </summary>
    public enum TShirtSizeVote
    {
        [Description("XS")]
        ExtraSmall,
        [Description("S")]
        Small,
        [Description("M")]
        Medium,
        [Description("L")]
        Large,
        [Description("XL")]
        ExtraLarge,
        [Description("?")]
        Unknown
    }
}
