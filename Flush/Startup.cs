using System;
using System.Text;
using System.Threading.Tasks;
using Flush.Contracts;
using Flush.Databases.Entities;
using Flush.Databases.Identity;
using Flush.Databases.Application;
using Flush.Application.Hubs;
using Flush.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace Flush
{
    /// <summary>
    /// Provides application configuration routines.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Construct a new instance of the Startup object.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// The configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure the runtime dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            var flushDbSection = Configuration.GetSection(Strings.Configuration.FlushDb);
            var identityDbSection = Configuration.GetSection(Strings.Configuration.IdentityDb);
            var jwtAuthenticationSection = Configuration.GetSection(Strings.Configuration.JwtAuthentication);
            var jwtSettings = jwtAuthenticationSection.Get<JwtOptions>();

            services.Configure<IdentityDatabaseOptions>(identityDbSection)
                .Configure<JwtOptions>(jwtAuthenticationSection)
                .Configure<FlushDatabaseOptions>(flushDbSection)
                .AddDbContext<IdentityContext>(ServiceLifetime.Singleton)
                .AddDbContext<ApplicationContext>(ServiceLifetime.Singleton)
                .AddTransient<AuthenticationProvider>()
                .AddSingleton<IDataStore2, ApplicationInMemoryDataStore>();


            services.AddSingleton<UserPurgeProvider>();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = Strings.Configuration.JwtIssuer,
                    ValidateAudience = false,
                    ValidAudience = Strings.Configuration.JwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(jwtSettings.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Configuration = new OpenIdConnectConfiguration();

                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = (m) =>
                    {
                        var accessToken = m.Request.Query[Strings.Configuration.AccessToken];

                        // If the request is for our hub...
                        var path = m.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            // Read the token out of the query string
                            m.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            services.AddRazorPages()
                .AddControllersAsServices();

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromMinutes(60);
            });
        }

        /// <summary>
        /// Configures the application HTTP pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(Strings.Configuration.ErrorEndpoint);
                app.UseHsts();
            }

            app.UsePathBase(Strings.Configuration.ApplicationBaseUri);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapHub<SessionHub>(Strings.Configuration.SessionHubEndpoint);
            });

            // We have to request the purge provider at least once, to ensure it
            // gets initialised.
            var _ = app.ApplicationServices.GetRequiredService<UserPurgeProvider>();
        }
    }
}
