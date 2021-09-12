using ScrumPokerClub.Services.Requests;
using ScrumPokerClub.Services.Responses;
using System.Threading.Tasks;

namespace ScrumPokerClub.Services
{
    /// <summary>
    /// Interface describing functionality of a global session manager.
    /// </summary>
    interface ISessionManagementService
    {
        /// <summary>
        /// Ensure that a session is appropriate configured.
        /// </summary>
        /// <param name="configureSessionRequest">The context.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// If the session is not found, it is created.
        /// </remarks>
        Task EnsureSessionConfiguredAsync(ConfigureSessionRequest configureSessionRequest);

        /// <summary>
        /// Join a session.
        /// </summary>
        /// <param name="joinSessionRequest">The context.</param>
        /// <returns>Nothing.</returns>
        Task JoinSessionAsync(JoinSessionRequest joinSessionRequest);

        /// <summary>
        /// Get the state of a session in progress.
        /// </summary>
        /// <param name="sessionStateRequest">The context.</param>
        /// <returns>The state, or null if no session found.</returns>
        Task<SessionStateResponse> GetSessionStateAsync(SessionStateRequest sessionStateRequest);

        /// <summary>
        /// Leave a session.
        /// </summary>
        /// <param name="leaveSessionRequest">The context.</param>
        /// <returns>Nothing.</returns>
        Task LeaveSessionAsync(LeaveSessionRequest leaveSessionRequest);

        /// <summary>
        /// Updates a player vote.
        /// </summary>
        /// <param name="updateVoteRequest">The context.</param>
        /// <returns>Nothing.</returns>
        Task UpdateVoteAsync(UpdateVoteRequest updateVoteRequest);

        /// <summary>
        /// Transition the server model to results phase.
        /// </summary>
        /// <param name="transitionToResultsPhaseRequest">The context.</param>
        /// <returns>Nothing.</returns>
        Task TransitionToResultsPhaseAsync(TransitionToResultsPhaseRequest transitionToResultsPhaseRequest);

        /// <summary>
        /// Transitions the server model to play phase.
        /// </summary>
        /// <param name="transitionToPlayPhaseRequest">The context.</param>
        /// <returns>Nothing.</returns>
        Task TransitionToPlayPhaseAsync(TransitionToPlayPhaseRequest transitionToPlayPhaseRequest);

        /// <summary>
        /// Updates a  player.
        /// </summary>
        /// <param name="updateParticipantRequest">The context.</param>
        /// <returns>Nothing.</returns>
        Task UpdateParticipantAsync(UpdateParticipantRequest updateParticipantRequest);
    }
}
