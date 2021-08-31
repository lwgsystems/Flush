using Microsoft.AspNetCore.Components;

namespace ScrumPokerClub.Shared
{
    public partial class PokerCard : ComponentBase
    {
        [Parameter]
        public object Value { get; set; }

        [Parameter]
        public EventCallback<PokerCard> OnClickCallback { get; set; }

        string activeClass = "";

        internal void SetIsTop(bool isTop)
        {
            activeClass = isTop ? "active" : string.Empty;
        }
    }
}
