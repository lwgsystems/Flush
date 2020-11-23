using System.ComponentModel;

namespace Flush.Data.Game.Model
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
