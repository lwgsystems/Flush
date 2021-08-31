using Microsoft.AspNetCore.Components;

namespace ScrumPokerClub.Shared
{
    public partial class NavMenu : ComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /*
        private bool collapseNavMenu = true;

        private string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }
        */
    }
}
