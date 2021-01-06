using System.ComponentModel;

namespace Flush.Databases.Entities
{
    /// <summary>
    /// Models the modified fibonacci voting scheme.
    /// </summary>
    public enum ModifiedFibonacciVote : int
    {
        [Description("0")]
        Zero,
        [Description("½")]
        Half,
        [Description("1")]
        One,
        [Description("2")]
        Two,
        [Description("3")]
        Three,
        [Description("5")]
        Five,
        [Description("8")]
        Eight,
        [Description("13")]
        Thirteen,
        [Description("21")]
        TwentyOne,
        [Description("40")]
        Forty,
        [Description("100")]
        OneHundred,
        [Description("?")]
        Unknown
    }
}
