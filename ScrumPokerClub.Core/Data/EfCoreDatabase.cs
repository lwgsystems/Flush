using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScrumPokerClub.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScrumPokerClub.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class EfCoreDatabase : IDatabase
    {
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, SessionContext> Sessions { get; init; }

        /// <summary>
        /// 
        /// </summary>
        private ILogger Logger { get; init; }

        /// <summary>
        /// 
        /// </summary>
        private SpcDbContext Context { get; init; }

        public EfCoreDatabase(
            ILogger<EfCoreDatabase> logger,
            SpcDbContext context)
        {
            Logger = logger;
            Context = context;

            Sessions = new();
            Context.Database.Migrate();
        }

        /// <inheritdoc/>
        public async Task<Profile> GetProfileAsync(string profileId)
        {
            Logger.LogDebug($"Fetching profile {profileId}.");
            return await Context.Profiles.FindAsync(profileId);
        }

        /// <inheritdoc/>
        public async Task UpsertProfileAsync(Profile profile)
        {
            Logger.LogDebug($"Upserting profile {profile.Id}.");

            var temp = Context.Profiles.FirstOrDefault(p => p.Id == profile.Id);
            if (temp is not null)
            {
                Context.Profiles.Update(profile);
            }
            else
            {
                Context.Profiles.Add(profile);
            }

            await Context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpsertPlayerContextAsync(string sessionId, string profileId, PlayerContext player)
        {
            Logger.LogDebug($"Upserting player context {sessionId}-{profileId}.");
            if (!Sessions.TryGetValue(sessionId, out SessionContext sessionContext))
                return;

            var existing = sessionContext.Players.FirstOrDefault(pc => pc.Profile.Id == player.Profile.Id);
            if (existing is not null)
                sessionContext.Players.Remove(existing);

            await Task.CompletedTask;
            sessionContext.Players.Add(player);
        }

        /// <inheritdoc/>
        public async Task<PlayerContext> GetPlayerContextAsync(string sessionId, string profileId)
        {
            Logger.LogDebug($"Fetching player context {sessionId}-{profileId}.");
            if (!Sessions.TryGetValue(sessionId, out SessionContext sessionContext))
                return default;

            var existing = sessionContext.Players.FirstOrDefault(pc => pc.Profile.Id == profileId);
            return await Task.FromResult(existing);
        }

        /// <inheritdoc/>
        public async Task SetVoteAsync(string sessionId, string profileId, int? vote)
        {
            Logger.LogDebug($"Setting vote {vote} on player {sessionId}-{profileId}.");
            if (!Sessions.TryGetValue(sessionId, out SessionContext sessionContext))
                return;

            var existing = sessionContext.Players.FirstOrDefault(pc => pc.Profile.Id == profileId);
            if (existing is null)
                return;

            await Task.CompletedTask;
            existing.LastVote = vote;
        }

        /// <inheritdoc/>
        public async Task<int?> GetVoteAsync(string sessionId, string profileId)
        {
            Logger.LogDebug($"Fetching vote from player {sessionId}-{profileId}.");
            if (!Sessions.TryGetValue(sessionId, out SessionContext sessionContext))
                return default;

            var existing = sessionContext.Players.FirstOrDefault(pc => pc.Profile.Id == profileId);
            if (existing is null)
                return default;

            return await Task.FromResult(existing.LastVote);
        }

        /// <inheritdoc/>
        public async Task SetGamePhaseAsync(string sessionId, GamePhase gamePhase)
        {
            Logger.LogDebug($"Setting session {sessionId} game phase to {gamePhase}.");
            if (!Sessions.TryGetValue(sessionId, out SessionContext sessionContext))
                return;

            await Task.CompletedTask;
            sessionContext.GamePhase = gamePhase;
        }

        /// <inheritdoc/>
        public async Task<GamePhase> GetGamePhaseAsync(string sessionId)
        {
            Logger.LogDebug($"Getting game phase of session {sessionId}.");
            if (!Sessions.TryGetValue(sessionId, out SessionContext sessionContext))
                return GamePhase.Unknown;

            return await Task.FromResult(sessionContext.GamePhase);
        }

        /// <inheritdoc/>
        public async Task AddToSessionContextAsync(string sessionId, string profileId, IUserInfoService userInfoService)
        {
            Logger.LogDebug($"Adding player context {sessionId}-{profileId}.");
            if (!Sessions.TryGetValue(sessionId, out SessionContext sessionContext))
                return;

            var existing = sessionContext.Players.FirstOrDefault(pc => pc.Profile.Id == profileId);
            if (existing is not null)
                return;

            var profile = await GetProfileAsync(profileId);
            sessionContext.Players.Add(new PlayerContext(userInfoService, profile));
        }

        /// <inheritdoc/>
        public async Task RemoveFromSessionContextAsync(string sessionId, string profileId)
        {
            Logger.LogDebug($"Removing player context {sessionId}-{profileId}.");
            if (!Sessions.TryGetValue(sessionId, out SessionContext sessionContext))
                return;

            await Task.CompletedTask;
            var existing = sessionContext.Players.FirstOrDefault(pc => pc.Profile.Id == profileId);
            if (existing is not null)
                sessionContext.Players.Remove(existing);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PlayerContext>> GetAllPlayerContextsInSessionContextAsync(string sessionId)
        {
            Logger.LogDebug($"Fetching all player contexts from session {sessionId}.");
            if (!Sessions.TryGetValue(sessionId, out SessionContext sessionContext))
                return await Task.FromResult(Enumerable.Empty<PlayerContext>());

            return await Task.FromResult(sessionContext.Players);
        }

        /// <inheritdoc/>
        public async Task<Session> GetSessionAsync(string sessionId)
        {
            Logger.LogDebug($"Fetching session {sessionId}.");

            Session session = default;
            if (ulong.TryParse(sessionId, out ulong id))
            {
                session = Context.Sessions.FirstOrDefault(s => s.Id == id);
            }

            if (session is null)
            {
                session = Context.Sessions.FirstOrDefault(s => s.Name == sessionId);
            }

            return await Task.FromResult(session);
        }

        /// <inheritdoc/>
        public async Task UpsertSessionAsync(Session session)
        {
            Logger.LogDebug($"Upserting session {session.Id}.");

            var temp = Context.Sessions.FirstOrDefault(s => s.Id == session.Id);
            if (temp is not null)
            {
                Context.Sessions.Update(session);
            }
            else
            {
                Context.Sessions.Add(session);
            }

            await Context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task EnsureSessionContextExistsAsync(string sessionId)
        {
            await Task.CompletedTask;
            if (!Sessions.TryGetValue(sessionId, out SessionContext _))
            {
                var sessionContext = new SessionContext() { GamePhase = GamePhase.Created };
                Sessions.Add(sessionId, sessionContext);
            }
        }
    }
}
