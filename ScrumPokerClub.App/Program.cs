using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ScrumPokerClub
{
    /// <summary>
    /// Entrypoint.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entrypoint.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        /// <summary>
        /// Create a hostbuilder for Spc.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The configured hostbuilder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((c, l) =>
                {
                    l.AddConfiguration(c.Configuration);
                    l.AddSentry();
                });

        /// <summary>
        /// Get the version string.
        /// </summary>
        public static string FriendlyVersion =>
            FileVersionInfo.GetVersionInfo(typeof(ScrumPokerClub.Program).Assembly.Location).ProductVersion;

        /// <summary>
        /// Get the build date and time.
        /// </summary>
        public static string BuildDateTime =>
            File.GetCreationTime(typeof(ScrumPokerClub.Program).Assembly.Location).ToUniversalTime().ToString();

    }
}
