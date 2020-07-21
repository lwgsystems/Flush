using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Flush.Data.Game.InMemory;
using Flush.Hubs;
using Flush.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flush.Providers
{
    /// <summary>
    /// A *very* bad mechanism for automatically logging out users when they've
    /// been disconnected for n seconds.
    /// </summary>
    public class AutomaticLogoutProvider
    {
        /// <summary>
        /// The time before a user is logged out on disconnect.
        /// </summary>
        private readonly int TIMEOUT_MILLISECONDS = 10000;

        /// <summary>
        /// A logger for use by this instance.
        /// </summary>
        private readonly ILogger<AutomaticLogoutProvider> _logger;
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// A mapping of active cancellation tokens to users.
        /// </summary>
        private Dictionary<string, CancellationTokenSource> _tasks =
            new Dictionary<string, CancellationTokenSource>();

        /// <summary>
        /// Constructs a new instance of the Automatic Logout Provider
        /// </summary>
        /// <param name="logger"></param>
        public AutomaticLogoutProvider(ILogger<AutomaticLogoutProvider> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        private AuthenticationProvider Provider { get; set; }

        /// <summary>
        /// Schedule a new automatic log out.
        /// </summary>
        /// <param name="user">The user to log out.</param>
        public void Add(string user)
        {
            _logger.LogDebug($"Enter {nameof(Add)}");

            if (!_tasks.ContainsKey(user))
            {
                var cts = new CancellationTokenSource();

                // fire and forget a task to logout the user.
                // if the user relogs before the delay is reached,
                // the task will be cancelled and no further action is taken.
                Task.Factory.StartNew(
                    () => AutomaticallyLogout(user, cts.Token),
                    cts.Token);

                // record the token source for later cancel.
                _tasks.Add(user, cts);
            }

            _logger.LogDebug($"Exit {nameof(Add)}");
        }

        /// <summary>
        /// If scheduled, cancel an automatic log out.
        /// </summary>
        /// <param name="user">The user.</param>
        public void Cancel(string user)
        {
            _logger.LogDebug($"Enter {nameof(Cancel)}");

            if (_tasks.ContainsKey(user))
            {
                // flag to the associated task that the user should be logged out.
                _tasks[user].Cancel();
                _tasks.Remove(user);
            }

            _logger.LogDebug($"Exit {nameof(Cancel)}");
        }

        /// <summary>
        /// Functionality for automatically logging out the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="notify">The notification delegate.</param>
        /// <param name="token">The cancellation token.</param>
        private async void AutomaticallyLogout(string user,
            CancellationToken token)
        {
            await Task.Delay(TIMEOUT_MILLISECONDS);
            if (!token.IsCancellationRequested)
            {
                _logger.LogInformation($"The user '{user}' has not reconnected after {TIMEOUT_MILLISECONDS / 1000}s. Logging out.");

                // We only need an Authentication Provider during this call,
                // so we create a scope (such that it may be disposed) and do
                // the work requiring the AP. See notes 20/08/20 p2.
                using (var scope = _serviceProvider.CreateScope())
                {
                    // perform the log out.
                    var scopedAuthProvider = scope.ServiceProvider
                        .GetRequiredService<AuthenticationProvider>();
                    await scopedAuthProvider
                        .Logout(new LoginCredentials { Email = user.Replace('_', '@') });

                    // notify everyone else.
                    var hubContext = scope.ServiceProvider
                        .GetRequiredService<IHubContext<PokerGameHub>>();
                    var inMemoryDataStore = scope.ServiceProvider
                        .GetRequiredService<InMemoryDataStore>();
                    await Helpers.LeaveRoom(user, inMemoryDataStore, hubContext);
                }
            }
        }
    }
}
