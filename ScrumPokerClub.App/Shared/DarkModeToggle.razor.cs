using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace ScrumPokerClub.Shared
{
    public partial class DarkModeToggle : ComponentBase
    {
        private static readonly string CLASS = "class";
        private static readonly string HREF = "href";
        private static readonly string LIGHT = "light";
        private static readonly string DARK = "dark";
        private static readonly string DARK_MODE_PREFERENCE = "darkModePreference";
        private static readonly string RADZEN_SELECTOR = "#radzen-stylesheet";
        private static readonly string RADZEN_BASE = "_content/Radzen.Blazor/css";
        private static readonly string RADZEN_LIGHT = $"{RADZEN_BASE}/humanistic-base.css";
        private static readonly string RADZEN_DARK = $"{RADZEN_BASE}/dark-base.css";
        private static string PREFERS_COLOR_SCHEME(string scheme) => $"(prefers-color-scheme: {scheme})";

        private string darkModePreference = LIGHT;

        public bool IsDarkMode
        {
            get => darkModePreference.Equals(DARK);
            set => darkModePreference = value ? DARK : LIGHT;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                darkModePreference = await JSInterop.GetItemFromStorageAsync<string>(DARK_MODE_PREFERENCE);
                if (string.IsNullOrWhiteSpace(darkModePreference))
                {
                    var mediaQueryList = await JSInterop.MatchMediaAsync(PREFERS_COLOR_SCHEME(DARK));
                    darkModePreference = mediaQueryList.Matches ? DARK : LIGHT;
                }

                await OnToggle();
            }
        }

        async Task OnToggle()
        {
            await JSInterop.PutItemInStorageAsync(DARK_MODE_PREFERENCE, darkModePreference);
            await JSInterop.SetHtmlAttribute(CLASS, darkModePreference);
            await JSInterop.SetAttribute(RADZEN_SELECTOR, HREF,
                IsDarkMode ? RADZEN_DARK : RADZEN_LIGHT);

            await InvokeAsync(() => StateHasChanged());
        }
    }
}
