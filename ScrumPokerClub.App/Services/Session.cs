using ScrumPokerClub.Services.Responses;
using System;
using System.Threading.Tasks;

namespace ScrumPokerClub.Services
{
    /// <summary>
    /// A generic play session.
    /// </summary>
    class Session : ISession
    {
        /// <summary>
        /// The number of participants currently active.
        /// </summary>
        private int participantCount;

        /// <inheritdoc/>
        public bool IsEmpty => participantCount == 0;

        /// <inheritdoc/>
        public event EventHandler<VoteUpdatedResponse> VoteUpdated;

        /// <inheritdoc/>
        public event EventHandler<PlayerConnectedResponse> PlayerConnected;

        /// <inheritdoc/>
        public event EventHandler<PlayerDisconnectedResponse> PlayerDisconnected;

        /// <inheritdoc/>
        public event EventHandler<TransitionToResultsResponse> TransitionToResults;

        /// <inheritdoc/>
        public event EventHandler<TransitionToPlayResponse> TransitionToPlay;

        /// <inheritdoc/>
        public event EventHandler<PlayerUpdatedResponse> PlayerUpdated;

        /// <inheritdoc/>
        public void Decrement()
        {
            participantCount = Math.Max(0, participantCount - 1);
        }

        /// <inheritdoc/>
        public void Increment()
        {
            participantCount = Math.Min(int.MaxValue, participantCount + 1);
        }

        /// <inheritdoc/>
        public async Task RaisePlayerConnectedAsync(PlayerConnectedResponse playerConnectedResponse)
            => await RaiseEvent(PlayerConnected, playerConnectedResponse);

        /// <inheritdoc/>
        public async Task RaisePlayerDisconnectedAsync(PlayerDisconnectedResponse playerDisconnectedResponse)
            => await RaiseEvent(PlayerDisconnected, playerDisconnectedResponse);

        /// <inheritdoc/>
        public async Task RaisePlayerUpdatedAsync(PlayerUpdatedResponse playerUpdatedResponse)
            => await RaiseEvent(PlayerUpdated, playerUpdatedResponse);

        /// <inheritdoc/>
        public async Task RaiseTransitionToPlayAsync(TransitionToPlayResponse transitionToPlayResponse)
            => await RaiseEvent(TransitionToPlay, transitionToPlayResponse);

        /// <inheritdoc/>
        public async Task RaiseTransitionToResultsAsync(TransitionToResultsResponse transitionToResultsResponse)
            => await RaiseEvent(TransitionToResults, transitionToResultsResponse);

        /// <inheritdoc/>
        public async Task RaiseVoteUpdatedAsync(VoteUpdatedResponse voteUpdatedResponse)
            => await RaiseEvent(VoteUpdated, voteUpdatedResponse);

        /// <summary>
        /// Template function for raising events.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event args.</typeparam>
        /// <param name="eventHandlerToRaise">The event.</param>
        /// <param name="eventArgs">The event args.</param>
        /// <returns>Nothing.</returns>
        private async Task RaiseEvent<TEventArgs>(EventHandler<TEventArgs> eventHandlerToRaise, TEventArgs eventArgs)
        {
            var invocationList = eventHandlerToRaise?.GetInvocationList();
            if (invocationList is null)
                return;

            foreach (var invocation in invocationList)
                await Task.Run(() => invocation.DynamicInvoke(this, eventArgs));
        }
    }
}
