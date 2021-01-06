using System.Collections.Generic;
using System.Linq;
using Flush.Databases.Entities;
using Flush.Extensions;

namespace Flush.Application.Hubs.Responses
{
    public class PlayerConnectedRequiresGameStateResponse
    {
        public class PlayerData
        {
            public string PlayerID { get; set; }
            public string Player { get; set; }
            public int? Vote { get; set; }
            public int AvatarID { get; set; }
            public bool IsModerator { get; set; }
            public bool IsObserver { get; set; }
        }

        public GamePhase Phase { get; set; }
        public IEnumerable<PlayerData> Players { get; set; }

        public int? Low => Players.MinOrDefault(pv => pv.Vote);
        public int? High => Players.MaxOrDefault(pv => pv.Vote);
        public int? Mode => Players.GroupBy(pv => pv.Vote)
            .OrderByDescending(grp => grp.Count())
            .Select(grp => grp.Key)
            .FirstOrDefault();
    }
}
