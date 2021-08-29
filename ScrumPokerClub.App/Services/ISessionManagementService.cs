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
        /// 
        /// </summary>
        /// <param name="configureSessionRequest"></param>
        /// <returns></returns>
        Task EnsureSessionConfiguredAsync(ConfigureSessionRequest configureSessionRequest);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="preconnectAction"></param>
        /// <returns></returns>
        Task JoinSessionAsync(JoinSessionRequest joinSessionRequest);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionStateRequest"></param>
        /// <returns></returns>
        Task<SessionStateResponse> GetSessionStateAsync(SessionStateRequest sessionStateRequest);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="predisconnectAction"></param>
        /// <returns></returns>
        Task LeaveSessionAsync(LeaveSessionRequest leaveSessionRequest);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateVoteRequest"></param>
        /// <returns></returns>
        Task UpdateVoteAsync(UpdateVoteRequest updateVoteRequest);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transitionToResultsPhaseRequest"></param>
        /// <returns></returns>
        Task TransitionToResultsPhaseAsync(TransitionToResultsPhaseRequest transitionToResultsPhaseRequest);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transitionToPlayPhaseRequest"></param>
        /// <returns></returns>
        Task TransitionToPlayPhaseAsync(TransitionToPlayPhaseRequest transitionToPlayPhaseRequest);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateParticipantRequest"></param>
        /// <returns></returns>
        Task UpdateParticipantAsync(UpdateParticipantRequest updateParticipantRequest);
    }
}
