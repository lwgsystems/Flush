using System;

namespace ScrumPokerClub
{
    /// <summary>
    /// Raised when an exception occurs in the data access layers.
    /// </summary>
    public class SpcDataException : SpcExceptionBase
    {
        public SpcDataException(string message)
            : base(message)
        {

        }
    }
}
