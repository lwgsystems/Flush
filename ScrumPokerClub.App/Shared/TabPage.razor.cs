using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace ScrumPokerClub.Shared
{
    public partial class TabPage : ComponentBase
    {
        [CascadingParameter]
        private TabControl Parent { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (Parent == null)
                throw new ArgumentNullException(nameof(Parent), "TabPage must exist within a TabControl");

            await base.OnInitializedAsync();

            Parent.AddTabPage(this);
        }
    }
}
