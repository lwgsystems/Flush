using System;

namespace ScrumPokerClub
{
    /// <summary>
    /// Raised if a client-side, user-interaction exception occurs,
    /// </summary>
    public class SpcClientException : SpcExceptionBase
    {
        public SpcClientException(string message)
            : base(message)
        {

        }
    }
}
