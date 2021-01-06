using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flush.Databases.Entities
{
    /// <summary>
    /// Models a Player in a <see cref="Game"/> of Scrum Poker.
    /// Marshallable object <see cref="PlayerStateInfo"/> provide player info.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The Player ID.
        /// TODO: This should be an integer.
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// The Player name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Player's identity in ASP.NET Identity.
        /// </summary>
        public string AspNetIdentity { get; set; }

        /// <summary>
        /// The Player's vote.
        /// </summary>
        public int? Vote { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating that the Player has voted and is
        /// ready.
        /// </summary>
        [NotMapped]
        public bool Ready => Vote != null;

        /// <summary>
        /// Gets or Sets a value indicating that the Player is an observer.
        /// </summary>
        public bool IsObserver { get; set; }

        /// <summary>
        /// The ID of the <see cref="Game"/> that this Player is in.
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        /// The <see cref="Game"/> this Player is in.
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// The <see cref="AuditLog"/>'s associated with this player.
        /// </summary>
        public ICollection<AuditLog> AuditLogs { get; set; }
    }
}
