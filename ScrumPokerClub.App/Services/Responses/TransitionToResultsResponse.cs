using ScrumPokerClub.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ScrumPokerClub.Services.Responses
{
    class TransitionToResultsResponse
    {
        public IEnumerable<KeyValuePair<string, int>> Votes;

        public int? Low => Votes.MinOrDefault(kvp => kvp.Value);
        public int? High => Votes.MaxOrDefault(kvp => kvp.Value);
        public int? Mode => Votes.GroupBy(kvp => kvp.Value)
            .OrderByDescending(grp => grp.Count())
            .Select(grp => grp.Key)
            .FirstOrDefault();
    }
}
