using Microsoft.AspNetCore.Components;

namespace ScrumPokerClub.Pages
{
    public partial class Index : ComponentBase
    {
        [Parameter]
        public string Session { get; set; }
    }
}
