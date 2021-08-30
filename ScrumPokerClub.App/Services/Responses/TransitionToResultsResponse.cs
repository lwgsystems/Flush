using ScrumPokerClub.Data;
using ScrumPokerClub.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ScrumPokerClub.Services.Responses
{
    /// <summary>
    /// Play to results event response context.
    /// </summary>
    class TransitionToResultsResponse
    {
        /// <summary>
        /// The votes of all players.
        /// </summary>
        public IEnumerable<KeyValuePair<string, int>> Votes;

        /// <summary>
        /// The lowest vote, or null if no votes.
        /// </summary>
        public int? Low => Votes.MinOrDefault(kvp => kvp.Value);

        /// <summary>
        /// The highest vote, or null if no votes.
        /// </summary>
        public int? High => Votes.MaxOrDefault(kvp => kvp.Value);

        /// <summary>
        /// The most frequently occurring vote, or null if no votes.
        /// </summary>
        public int? Mode => Votes.GroupBy(kvp => kvp.Value)
            .OrderByDescending(grp => grp.Count())
            .Select(grp => grp.Key)
            .FirstOrDefault();

        /// <summary>
        /// Create a <see cref="TransitionToResultsResponse"/> from a set of player states.
        /// </summary>
        /// <param name="playerStates">The input states.</param>
        /// <returns>The populated response.</returns>
        public static TransitionToResultsResponse FromPlayerStates(IEnumerable<PlayerState> playerStates)
        {
            return new TransitionToResultsResponse()
            {
                Votes = playerStates
                    .Where(ps => ps.Vote.HasValue)
                    .Select(ps => new KeyValuePair<string, int>(ps.PlayerId, ps.Vote.Value))
            };
        }
    }
}
