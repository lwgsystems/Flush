using Microsoft.AspNetCore.Components;
using Radzen;
using System.Threading.Tasks;

namespace ScrumPokerClub.Shared
{
    public partial class NamePlate : ComponentBase
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Parameter]
        public int AvatarId { get; set; }

        [Parameter]
        public string Vote { get; set; }

        [Parameter]
        public bool IsModerator { get; set; }

        [Parameter]
        public EventCallback<NamePlate> OnClickCallback { get; set; }

        ElementReference avatar;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnInitializedAsync();
            if (userInfoService.Identifier != Id)
                return;

            if (firstRender)
            {
                tooltipService.Open(avatar,
                    "Click your avatar to edit your profile!",
                    new TooltipOptions()
                    {
                        Duration = 1000000,
                        Position = TooltipPosition.Left
                    });;
            }
        }

        private string NamePlateVoteClasses
        {
            get
            {
                var classes = string.Empty;

                if (string.IsNullOrWhiteSpace(Vote)) {; }
                else if (Vote.Equals("SENTINEL")) classes += "voted";
                else classes += "voted display-vote";

                classes += IsModerator ? " moderator" : string.Empty;

                return classes;
            }
        }

        void CloseTooltip()
        {
            if (userInfoService.Identifier != Id)
                return;

            tooltipService.Close();
        }
    }
}
