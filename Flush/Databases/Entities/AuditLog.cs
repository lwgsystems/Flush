using System;

namespace Flush.Databases.Entities
{
    /// <summary>
    /// Models a log of an <see cref="Action"/> taken by a <see cref="Player"/>
    /// in a <see cref="Game"/> of Scrum Poker.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// The AuditLog ID.
        /// </summary>
        public int AuditLogId { get; set; }

        /// <summary>
        /// The ID of the <see cref="Game"/> that this AuditLog is associated
        /// with.
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        /// The ID of the <see cref="Player"/> that this AuditLog is associated
        /// with.
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// The Date and Time of the Log.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The <see cref="Action"/> that was taken.
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        /// The Data associated with this log.
        /// </summary>
        public string Data { get; set; }


        /// <summary>
        /// The <see cref="Game"/> this AuditLog is associated with.
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// The <see cref="Player"/> this AuditLog is associated with.
        /// </summary>
        public Player Player { get; set; }
    }
}
