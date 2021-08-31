using ScrumPokerClub.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrumPokerClub.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class SessionContext
    {
        /// <summary>
        /// The meta info.
        /// </summary>
        public Session Session { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public GamePhase GamePhase { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PlayerContext> Players { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public SessionContext()
        {
            Players = new();
        }
    }
}
