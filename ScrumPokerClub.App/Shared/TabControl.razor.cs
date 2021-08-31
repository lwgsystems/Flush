using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScrumPokerClub.Shared
{
    public partial class TabControl : ComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public bool ShowTabs
        {
            set
            {
                tabVisibility = value ? "block" : "none";

                InvokeAsync(() => StateHasChanged());
            }
        }

        public TabPage Current { get; set; }

        IList<TabPage> tabPages;

        string tabVisibility;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            tabPages = new List<TabPage>();
        }

        internal void AddTabPage(TabPage tabPage)
        {
            tabPages.Add(tabPage);

            if (tabPages.Count == 1)
                Current = tabPage;

            InvokeAsync(() => StateHasChanged());
        }

        string GetButtonClass(TabPage tabPage)
        {
            return tabPage == Current ? "btn-primary" : "btn-secondary";
        }

        public void ActivatePage(TabPage tabPage)
        {
            if (!tabPages.Contains(tabPage))
                return;

            Current = tabPage;

            InvokeAsync(() => StateHasChanged());
        }
    }
}
