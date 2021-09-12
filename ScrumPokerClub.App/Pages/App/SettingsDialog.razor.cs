using Microsoft.AspNetCore.Components;
using ScrumPokerClub.Data;
using ScrumPokerClub.Services.Requests;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.IO;

namespace ScrumPokerClub.Pages.App
{
    public partial class SettingsDialog : ComponentBase, IDisposable
    {
        [Parameter]
        public string Session { get; set; }

        [Parameter]
        public string Id { get; set; }

        bool IsModerator { get; set; }

        static int NumAvatars =>
            Directory.GetFiles($"{Directory.GetCurrentDirectory()}/wwwroot/img/avatar/SVG").Length;

        PlayerContext playerContext;

        protected override async Task OnInitializedAsync()
        {
            await Task.CompletedTask;
            playerContext = await DataStore.GetPlayerContextAsync(Session, Id);
            IsModerator = playerContext.Moderating;
            Data = playerContext.DisplayName;

            aTimer = new Timer(1000);
            aTimer.Elapsed += OnUserFinish;
            aTimer.AutoReset = false;
        }

        void OnModeratorChanged()
        {
            SessionManagementService.UpdateParticipantAsync(new UpdateParticipantRequest()
            {
                Session = Session,
                IsModerator = IsModerator
            });
        }

        string IsSelectedAvatar(int avatarId)
        {
            return playerContext.Profile.AvatarId == avatarId ?
                "voted" : "";
        }

        async void OnAvatarChanged(int avatarId)
        {
            await SessionManagementService.UpdateParticipantAsync(new UpdateParticipantRequest()
            {
                Session = Session,
                AvatarId = avatarId
            });

            playerContext = await DataStore.GetPlayerContextAsync(Session, Id);
            await InvokeAsync(() => StateHasChanged());
        }

        // https://stackoverflow.com/questions/57533970/blazor-textfield-oninput-user-typing-delay
        public string Data { get; set; }
        private Timer aTimer;


        void OnHandleKeyUp()
        {
            // remove previous one
            aTimer.Stop();

            // new timer
            aTimer.Start();
        }

        async void OnResetDisplayName()
        {
            await SessionManagementService.UpdateParticipantAsync(new UpdateParticipantRequest()
            {
                Session = Session,
                DisplayName = "usemymicrosoftname"
            });

            Data = playerContext.DisplayName;
            StateHasChanged();
        }

        private void OnUserFinish(object source, ElapsedEventArgs e)
        {
            InvokeAsync(async () =>
            {
                // do nothing if empty
                if (string.IsNullOrEmpty(Data))
                    return;

                await SessionManagementService.UpdateParticipantAsync(new UpdateParticipantRequest()
                {
                    Session = Session,
                    DisplayName = Data
                });

                StateHasChanged();
            });
        }

        void IDisposable.Dispose()
        {
            aTimer?.Dispose();
        }
    }
}
