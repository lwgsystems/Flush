using Microsoft.AspNetCore.Components;
using ScrumPokerClub.Data;
using ScrumPokerClub.Services.Requests;
using System;
using System.Threading.Tasks;

namespace ScrumPokerClub.Shared
{
    public partial class PokerDeck : ComponentBase
    {
        [CascadingParameter]
        public string Session { get; set; }

        public Array Values { get; set; }

        PokerCard Current { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            Values = Enum.GetValues(typeof(ModifiedFibonacciVote));
        }

        internal void OnPokerCardClicked(PokerCard card)
        {
            Current?.SetIsTop(false);

            card.SetIsTop(true);

            Current = card;

            var vote = (int)Enum.Parse(typeof(ModifiedFibonacciVote), ((Enum)card.Value).ToString());
            sessionManagementService.UpdateVoteAsync(new UpdateVoteRequest()
            {
                Session = Session,
                Vote = $"{vote}"
            });

            InvokeAsync(() => StateHasChanged());
        }

        internal void SetTopCard(PokerCard card)
        {
            Current = card;
        }
    }
}
