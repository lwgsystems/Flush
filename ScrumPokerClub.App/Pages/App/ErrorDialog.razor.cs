using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ScrumPokerClub.Pages.App
{
    public partial class ErrorDialog : ComponentBase
    {
        private static readonly Dictionary<string, string> exceptionToFriendlyName = new()
        {
            { nameof(SpcClientException), "a client" },
            { nameof(SpcDataException), "a database" },
            { nameof(SpcIdentityException), "an authentication" },
            { nameof(SpcSecurityException), "a security" },
            { nameof(SpcSessionException), "a session" },
            { nameof(SpcExceptionBase), "an unknown"}
        };

        [Parameter]
        public SpcExceptionBase Exception { get; set; }

        string BasicErrorMessage =>
            $"We're sorry, {exceptionToFriendlyName[Exception.GetType().Name]} issue has occurred.";

        string DetailedErrorMessage =>
            $"{Exception.Message}";

        string Advice =>
            $"{Exception.Message}"; // todo replace with actual advice variable
    }
}
