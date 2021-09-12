using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using ScrumPokerClub.Data;
using ScrumPokerClub.Extensions;
using ScrumPokerClub.Services.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScrumPokerClub.Shared
{
    public partial class ResultsView : ComponentBase
    {
        [CascadingParameter]
        public string Session { get; set; }

        public string Min { get; set; }
        public string Mode { get; set; }
        public string Max { get; set; }
        public string TotalVotes { get; set; }

        class DataItem
        {
            public string Size { get; init; }
            public int Count { get; init; }
        }

        IList<DataItem> Votes { get; set; } = new List<DataItem>();

        internal async Task OnTransitionToResults(object _, TransitionToResultsResponse transitionToResultsResponse)
        {
            // todo this shouldn't be tied to a specific voting model
            Min = transitionToResultsResponse.Low.HasValue ? $"{((ModifiedFibonacciVote)transitionToResultsResponse.Low.Value).Description()}" : "?";
            Mode = transitionToResultsResponse.Mode.HasValue ? $"{((ModifiedFibonacciVote)transitionToResultsResponse.Mode.Value).Description()}" : "?";
            Max = transitionToResultsResponse.High.HasValue ? $"{((ModifiedFibonacciVote)transitionToResultsResponse.High.Value).Description()}" : "?";
            TotalVotes = $"{transitionToResultsResponse.Votes.Count()}";

            Votes.Clear();
            var counts = transitionToResultsResponse.Votes
                .GroupBy(kvp => kvp.Value);

            foreach (var count in counts)
            {
                Votes.Add(new DataItem()
                {
                    Size = ((ModifiedFibonacciVote)count.Key).Description(),
                    Count = count.Count()
                });
            }

            await InvokeAsync(() => StateHasChanged());
        }
    }
}
