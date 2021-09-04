using System;

namespace ScrumPokerClub
{
    /// <summary>
    /// Raised when an exception pertaining to authentication details occurs.
    /// </summary>
    public class SpcIdentityException : SpcExceptionBase
    {
        public SpcIdentityException(string message)
            : base(message)
        {

        }
    }
}
