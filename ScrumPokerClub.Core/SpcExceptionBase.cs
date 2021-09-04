using System;

namespace ScrumPokerClub
{
    /// <summary>
    /// Base class for Spc exceptions.
    /// </summary>
    public abstract class SpcExceptionBase : Exception
    {
        /// <summary>
        /// The session id.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// The user id.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Create a new instance of <see cref="SpcExceptionBase"/>.
        /// </summary>
        /// <param name="message"></param>
        public SpcExceptionBase(string message)
            : base(message)
        {

        }
    }
}
