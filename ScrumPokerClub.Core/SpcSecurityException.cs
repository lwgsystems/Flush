using System;

namespace ScrumPokerClub
{
    /// <summary>
    /// Raised when an exception pertaining to security occurs.
    /// </summary>
    public class SpcSecurityException : SpcExceptionBase
    {
        public SpcSecurityException(string message)
            : base(message)
        {

        }
    }
}
