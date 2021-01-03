using System;
using System.Net.Security;
using System.Security.Authentication;
using Flush.Data.Game.EfCore;
using Flush.Data.Identity;
using Flush.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Flush
{
    /// <summary>
    /// Flush application entrypoint.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entrypoint.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                CreateOrMigrateDatabase<FlushContext>(services);
                CreateOrMigrateDatabase<IdentityContext>(services);
            }
            host.Run();
        }

        /// <summary>
        /// Migrate the application database of type TContext, creating it if it
        /// does not exist.
        /// </summary>
        /// <typeparam name="TContext">The database context type.</typeparam>
        /// <param name="services">The service provider.</param>
        private static void CreateOrMigrateDatabase<TContext>(IServiceProvider services)
            where TContext : DbContext
        {
                try
                {
                    var context = services.GetRequiredService<TContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex,
                        "An error occurred during creation or migration of the {0} backing store.",
                        typeof(TContext).ToString());
                } 
        }

        /// <summary>
        /// Create an IHostBuilder instance with a configured Kestrel back-end.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The configured host builder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    if (Helpers.DevelopmentMode)
                    {
                        logging.ClearProviders();
                        logging.AddDebug();
                    } 
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    /*
                    webBuilder.ConfigureKestrel(opts =>
                    opts.ConfigureHttpsDefaults(httpsOpts =>
                    {
                        httpsOpts.SslProtocols = SslProtocols.Tls13 |
                                                 SslProtocols.Tls12;
                        httpsOpts.OnAuthenticate = (c, s) =>
                        {
                            s.CipherSuitesPolicy = new CipherSuitesPolicy(new[]
                            {
                                // TLS 1.3
                                TlsCipherSuite.TLS_CHACHA20_POLY1305_SHA256,
                                TlsCipherSuite.TLS_AES_128_GCM_SHA256,
                                TlsCipherSuite.TLS_AES_256_GCM_SHA384,
                                // TLS 1.2
                                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256,
                                TlsCipherSuite.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
                                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,
                                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
                                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
                                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_GCM_SHA384,
                                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256,

                            });
                        };
                    }));
                    */
                    webBuilder.UseStartup<Startup>();
                });
    }
}
