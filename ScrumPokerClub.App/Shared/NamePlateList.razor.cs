using Microsoft.AspNetCore.Components;
using Radzen;
using ScrumPokerClub.Data;
using ScrumPokerClub.Extensions;
using ScrumPokerClub.Pages.App;
using ScrumPokerClub.Services.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScrumPokerClub.Shared
{
    public partial class NamePlateList : ComponentBase
    {
        [CascadingParameter]
        public string Session { get; set; }

        private class NamePlateDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int AvatarId { get; set; }
            public string Vote { get; set; }
            public bool IsModerator { get; set; }
        }

        private IList<NamePlateDto> namePlatesDto;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            namePlatesDto = new List<NamePlateDto>();
        }

        // called when a player connects
        internal async Task OnPlayerConnected(object _, PlayerConnectedResponse playerConnectedResponse)
        {
            // bail early if there's already a nameplate for the connected player
            if (namePlatesDto.Any(npdto => npdto.Id == playerConnectedResponse.Id))
                return;

            var newNamePlateDto = new NamePlateDto()
            {
                Id = playerConnectedResponse.Id,
                Name = playerConnectedResponse.Name,
                AvatarId = playerConnectedResponse.AvatarId,
                IsModerator = playerConnectedResponse.IsModerator
            };

            namePlatesDto.Add(newNamePlateDto);

            await InvokeAsync(() => StateHasChanged());
        }

        // called when a player disconnects
        internal async Task OnPlayerDisconnected(object _, PlayerDisconnectedResponse playerDisconnectedResponse)
        {
            var namePlateDto = namePlatesDto.FirstOrDefault(npdto => npdto.Id == playerDisconnectedResponse.Id);
            if (namePlateDto is null)
                return;

            namePlatesDto.Remove(namePlateDto);

            await InvokeAsync(() => StateHasChanged());
        }

        /// called when a vote is updated.
        internal async Task OnVoteUpdated(object _, VoteUpdatedResponse voteUpdatedResponse)
        {
            var namePlateDto = namePlatesDto.FirstOrDefault(npdto => npdto.Id == voteUpdatedResponse.Id);
            if (namePlateDto is null)
                return;

            namePlateDto.Vote = "SENTINEL";

            await InvokeAsync(() => StateHasChanged());
        }

        internal async Task OnTransitionToPlay(object _, TransitionToPlayResponse __)
        {
            foreach (var namePlateDto in namePlatesDto)
                namePlateDto.Vote = string.Empty;

            await InvokeAsync(() => StateHasChanged());
        }

        internal async Task OnTransitionToResults(object _, TransitionToResultsResponse transitionToResultsResponse)
        {
            foreach (var vote in transitionToResultsResponse.Votes)
            {
                var npdto = namePlatesDto.FirstOrDefault(npdto => npdto.Id == vote.Key);
                if (npdto is null)
                    continue; // shouldn't happen

                npdto.Vote = ((ModifiedFibonacciVote)vote.Value).Description();
            }

            await InvokeAsync(() => StateHasChanged());
        }

        internal async Task OnPlayerUpdated(object _, PlayerUpdatedResponse playerUpdatedResponse)
        {
            var namePlateDto = namePlatesDto.FirstOrDefault(npdto => npdto.Id == playerUpdatedResponse.Id);
            if (namePlateDto is null)
                return;

            namePlateDto.IsModerator = playerUpdatedResponse.IsModerator;
            namePlateDto.AvatarId = playerUpdatedResponse.AvatarId;
            namePlateDto.Name = playerUpdatedResponse.DisplayName;

            await InvokeAsync(() => StateHasChanged());
        }

        internal async Task OnNamePlateClicked(NamePlate namePlate)
        {
            if (namePlate.Id != UserInfoService.Identifier)
                return;

            await DialogService.OpenAsync<SettingsDialog>($"Editing {UserInfoService.Name}'s Profile",
                new Dictionary<string, object>()
                {
                    { "Session", Session },
                    { "Id", UserInfoService.Identifier }
                }, new DialogOptions());

        }
    }
}
