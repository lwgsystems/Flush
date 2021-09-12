using Microsoft.AspNetCore.Components;
using Radzen;
using ScrumPokerClub.Data;
using ScrumPokerClub.Services.Requests;
using ScrumPokerClub.Services.Responses;
using ScrumPokerClub.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScrumPokerClub.Pages.App
{
    public partial class Play : ComponentBase, IDisposable
    {
        [Parameter]
        public string Session { get; set; }

        NamePlateList namePlateList;
        TabControl tabControl;
        TabPage playTab;
        TabPage resultsTab;
        ResultsView resultsView;
        FullScreenSpinner spinner;

        bool ModeratorToolBoxMasterEnable = false;

        protected override async Task OnInitializedAsync()
        {
            await SessionManagementService.EnsureSessionConfiguredAsync(new ConfigureSessionRequest()
            {
                Session = Session,
                Configure = session =>
                {
                    session.TransitionToResults += OnTransitionToResults;
                    session.TransitionToPlay += OnTransitionToPlay;
                    session.PlayerConnected += OnPlayerConnected;
                    session.PlayerDisconnected += OnPlayerDisconnected;
                    session.VoteUpdated += OnVoteUpdated;
                    session.PlayerUpdated += OnPlayerUpdated;
                }
            });

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            await SessionManagementService.JoinSessionAsync(new JoinSessionRequest()
            {
                Session = Session
            });

            var sessionStateResponse = await SessionManagementService.GetSessionStateAsync(new SessionStateRequest()
            {
                Session = Session
            });

            ReconcilePlayers(sessionStateResponse);
            switch (sessionStateResponse.GamePhase)
            {
                case GamePhase.Voting:
                    ReconcileVotingPhase(sessionStateResponse);
                    break;

                case GamePhase.Results:
                    ReconcileResultsPhase(sessionStateResponse);
                    break;

                default:
                    break;
            }

            spinner.Remove();

            await InvokeAsync(() => StateHasChanged());
        }

        async void OnTransitionToResults(object sender, TransitionToResultsResponse transitionToResultsResponse)
        {
            await Task.CompletedTask;
            tabControl.ActivatePage(resultsTab);

            await namePlateList.OnTransitionToResults(sender, transitionToResultsResponse);
            await resultsView.OnTransitionToResults(sender, transitionToResultsResponse);
        }

        async void OnTransitionToPlay(object sender, TransitionToPlayResponse transitionToPlayResponse)
        {
            await namePlateList.OnTransitionToPlay(sender, transitionToPlayResponse);

            tabControl.ActivatePage(playTab);
        }

        async void OnPlayerConnected(object sender, PlayerConnectedResponse playerConnectedResponse)
        {
            await namePlateList.OnPlayerConnected(sender, playerConnectedResponse);
        }

        async void OnPlayerDisconnected(object sender, PlayerDisconnectedResponse playerDisconnectedResponse)
        {
            // todo account for weirdness
            if (UserInfoService.Identifier == playerDisconnectedResponse.Id)
            {
                await DialogService.OpenAsync<ErrorDialog>($"ERROR",
                    new Dictionary<string, object>()
                    {
                        { "Exception", new SpcSessionException("You were disconnected.") }
                    }, new DialogOptions()
                    {
                        ShowClose = false
                    });
            }

            await namePlateList.OnPlayerDisconnected(sender, playerDisconnectedResponse);
        }

        async void OnVoteUpdated(object sender, VoteUpdatedResponse voteUpdatedResponse)
        {
            await namePlateList.OnVoteUpdated(sender, voteUpdatedResponse);
        }

        async void OnPlayerUpdated(object sender, PlayerUpdatedResponse playerUpdatedResponse)
        {
            await namePlateList.OnPlayerUpdated(sender, playerUpdatedResponse);

            if (UserInfoService.Identifier == playerUpdatedResponse.Id)
                ModeratorToolBoxMasterEnable = playerUpdatedResponse.IsModerator;

            await InvokeAsync(() => StateHasChanged());
        }

        public void Dispose()
        {
            SessionManagementService.EnsureSessionConfiguredAsync(new ConfigureSessionRequest()
            {
                Session = Session,
                Configure = session =>
                {
                    session.TransitionToResults -= OnTransitionToResults;
                    session.TransitionToPlay -= OnTransitionToPlay;
                    session.PlayerConnected -= OnPlayerConnected;
                    session.PlayerDisconnected -= OnPlayerDisconnected;
                    session.VoteUpdated -= OnVoteUpdated;
                    session.PlayerUpdated -= OnPlayerUpdated;
                }
            });

            SessionManagementService.LeaveSessionAsync(new LeaveSessionRequest()
            {
                Session = Session,
                Id = UserInfoService.Identifier
            });
        }

        void ReconcilePlayers(SessionStateResponse sessionStateResponse)
        {
            foreach (var player in sessionStateResponse.Players)
            {
                OnPlayerConnected(this, new PlayerConnectedResponse()
                {
                    Id = player.Profile.Id,
                    Name = player.DisplayName,
                    AvatarId = player.Profile.AvatarId,
                    IsModerator = player.Moderating
                });

                if (UserInfoService.Identifier == player.Profile.Id)
                    ModeratorToolBoxMasterEnable = player.Moderating;
            }
        }

        void ReconcileVotingPhase(SessionStateResponse sessionStateResponse)
        {
            foreach (var player in sessionStateResponse.Players)
            {
                if (player.LastVote is not null)
                    OnVoteUpdated(this, new VoteUpdatedResponse() { Id = player.Profile.Id });
            }
        }

        void ReconcileResultsPhase(SessionStateResponse sessionStateResponse)
        {
            OnTransitionToResults(this, TransitionToResultsResponse.FromPlayerStates(sessionStateResponse.Players));
        }
    }
}
