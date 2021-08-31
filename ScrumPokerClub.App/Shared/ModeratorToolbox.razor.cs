using Microsoft.AspNetCore.Components;
using ScrumPokerClub.Services.Requests;

namespace ScrumPokerClub.Shared
{
    public partial class ModeratorToolbox : ComponentBase
    {
        [CascadingParameter]
        public string Session { get; set; }

        [Parameter]
        public bool EnableVotingTools { get; set; } = false;

        [Parameter]
        public bool EnableResultsTools { get; set; } = false;

        void RevealVotes()
        {
            sessionManagementService.TransitionToResultsPhaseAsync(new TransitionToResultsPhaseRequest()
            {
                Session = Session
            });
        }

        void PlayAgain()
        {
            sessionManagementService.TransitionToPlayPhaseAsync(new TransitionToPlayPhaseRequest()
            {
                Session = Session
            });
        }
    }
}
