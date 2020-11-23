using System.Collections.Generic;
using System.Linq;
using Flush.Extensions;

namespace Flush.Hubs.Responses
{
    public class SendResultResponse
    {
        public class VoteInfo
        {
            public string PlayerID { get; set; }
            public int Vote { get; set; }
        }

        public IEnumerable<VoteInfo> Votes { get; set; }

        public int? Low => Votes.MinOrDefault(pv => pv.Vote);
        public int? High => Votes.MaxOrDefault(pv => pv.Vote);
        public int? Mode => Votes.GroupBy(pv => pv.Vote)
            .OrderByDescending(grp => grp.Count())
            .Select(grp => grp.Key)
            .FirstOrDefault();
    }
}
