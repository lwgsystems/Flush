using ScrumPokerClub.Services.Responses;
using System;
using System.Threading.Tasks;

namespace ScrumPokerClub.Services
{
    /// <summary>
    /// Interface for game session implementations.
    /// </summary>
    interface ISession
    {
        /// <summary>
        /// Raised when a player connects, or reconnects.
        /// </summary>
        event EventHandler<PlayerConnectedResponse> PlayerConnected;
        Task RaisePlayerConnectedAsync(PlayerConnectedResponse playerConnectedResponse);

        /// <summary>
        /// Raised when a player disconnects.
        /// </summary>
        event EventHandler<PlayerDisconnectedResponse> PlayerDisconnected;
        Task RaisePlayerDisconnectedAsync(PlayerDisconnectedResponse playerDisconnectedResponse);

        /// <summary>
        /// Raised when a player updates their vote.
        /// </summary>
        event EventHandler<VoteUpdatedResponse> VoteUpdated;
        Task RaiseVoteUpdatedAsync(VoteUpdatedResponse voteUpdatedResponse);

        /// <summary>
        /// Raised when the session has transitioned to the results phase.
        /// </summary>
        event EventHandler<TransitionToResultsResponse> TransitionToResults;
        Task RaiseTransitionToResultsAsync(TransitionToResultsResponse transitionToResultsResponse);

        /// <summary>
        /// Raised when the session has transitioned to the play phase.
        /// </summary>
        event EventHandler<TransitionToPlayResponse> TransitionToPlay;
        Task RaiseTransitionToPlayAsync(TransitionToPlayResponse transitionToPlayResponse);

        /// <summary>
        /// Raised when a player is updated.
        /// </summary>
        event EventHandler<PlayerUpdatedResponse> PlayerUpdated;
        Task RaisePlayerUpdatedAsync(PlayerUpdatedResponse playerUpdatedResponse);

        /// <summary>
        /// True if the session has no participants, else false.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Increment the participant counter.
        /// </summary>
        void Increment();

        /// <summary>
        /// Decrement the participant counter.
        /// </summary>
        void Decrement();
    }
}
