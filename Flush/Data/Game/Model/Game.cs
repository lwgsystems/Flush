using System.Collections.Generic;

namespace Flush.Data.Game.Model
{
    /// <summary>
    /// Models a Game of Scrum Poker.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// The Game ID.
        /// </summary>
        /// <remarks>
        /// This is also the name.
        /// </remarks>
        public string GameId { get; set; }

        /// <summary>
        /// The current state of this Game.
        /// </summary>
        public GamePhase Phase { get; set; } = GamePhase.Created;

        /// <summary>
        /// The current voting model of this Game.
        /// </summary>
        public VotingModel Model { get; set; } = VotingModel.ModifiedFibonacci;

        /// <summary>
        /// The <see cref="Player"/>'s in this Game.
        /// </summary>
        public ICollection<Player> Players { get; set; }

        /// <summary>
        /// The <see cref="AuditLog"/>'s associated with this Game.
        /// </summary>
        public ICollection<AuditLog> AuditLogs { get; set; }
    }
}
