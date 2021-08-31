using Microsoft.AspNetCore.Components;
using Radzen;
using System.Threading.Tasks;

namespace ScrumPokerClub.Shared
{
    public partial class FullScreenSpinner : ComponentBase
    {
        string display = "";

        ProgressBarMode pbarMode = ProgressBarMode.Indeterminate;

        internal void Remove()
        {
            display = "transition";
            pbarMode = ProgressBarMode.Determinate;
            Task.Delay(1000)
                .ContinueWith(t => InvokeAsync(() => StateHasChanged()));
        }
    }
}
