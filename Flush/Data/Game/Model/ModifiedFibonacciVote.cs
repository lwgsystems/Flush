namespace Flush.Data.Game.Model
{
    /// <summary>
    /// Models the modified fibonacci voting scheme.
    /// </summary>
    public enum ModifiedFibonacciVote : int
    {
        NoVote = -1,
        Zero,
        Half,
        One,
        Two,
        Three,
        Five,
        Eight,
        Thirteen,
        TwentyOne,
        Forty,
        OneHundred,
        Unknown
    }
}
