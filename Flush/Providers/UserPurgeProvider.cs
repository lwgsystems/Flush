using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Flush.Data.Game;
using Flush.Hubs;
using Microsoft.AspNetCore.SignalR;
using Flush.Hubs.Responses;

namespace Flush.Providers
{
    public class UserPurgeProvider
    {
        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan delay = TimeSpan.FromSeconds(10);

        private Timer timer;
        private ILogger<UserPurgeProvider> logger;
        private IServiceProvider serviceProvider;

        public UserPurgeProvider(ILogger<UserPurgeProvider> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;

            // Runs the guest player purge every 30 seconds.
            timer = new Timer(async (object _) =>
            {
                await DoPurge();
            }, null, delay, delay);
        }

        private async Task DoPurge()
        {
            logger.LogDebug("Starting guest user purge.");
            using (var scope = serviceProvider.CreateScope())
            {
                var authenticationProvider = scope.ServiceProvider.GetRequiredService<AuthenticationProvider>();
                var dataStore = scope.ServiceProvider.GetRequiredService<IDataStore2>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<PokerGameHub>>();

                foreach (var kvp in dataStore.GetDisconnectedPlayers())
                {
                    if (kvp.Value.Add(timeout) <= DateTime.Now)
                    {
                        logger.LogDebug($"Purging guest user {kvp.Key}.");

                        // Inform the other players that this player has disconnected.
                        var group = dataStore.GetPlayerState(kvp.Key).Group;
                        await hubContext.Clients
                            .Group(group)
                            .SendAsync("PlayerPurged", new PlayerPurgedResponse()
                            {
                                PlayerID = kvp.Key
                            });

                        // Finally, purge them from the identity store.
                        dataStore.RemovePlayer(kvp.Key);
                        await authenticationProvider.Logout(kvp.Key);
                    }
                }
            }
            logger.LogDebug("Finished guest user purge.");
        }
    }
}
