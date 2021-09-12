using System;

namespace ScrumPokerClub
{
    /// <summary>
    /// Raised if a session-related exception occurs.
    /// </summary>
    public class SpcSessionException : SpcExceptionBase
    {
        public SpcSessionException(string message)
            : base(message)
        {

        }
    }
}
