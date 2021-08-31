using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Radzen;
using ScrumPokerClub.Data;
using ScrumPokerClub.Interop;
using ScrumPokerClub.Services;
using Snowflake.Core;
using System;

namespace ScrumPokerClub
{
    /// <summary>
    /// Service configuration.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Create a new instance of <see cref="Startup"/>.
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

        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));
            services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();

            services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy
                options.FallbackPolicy = options.DefaultPolicy;
            });

            services.AddRazorPages();
            services.AddServerSideBlazor()
                .AddMicrosoftIdentityConsentHandler();

            services
                .AddScoped<TooltipService>()
                .AddScoped<DialogService>();

            services
                .AddHttpContextAccessor()
                .AddScoped<IJSInterop, JSInterop>()
                .AddScoped<IUserInfoService, UserInfoService>()
                .AddSingleton<ISessionManagementService, SessionManagementService>()
                .AddDbContext<SpcContext>(ServiceLifetime.Singleton)
                .AddSingleton<IDatabase, EfCoreDatabase>()
                .AddSingleton<ISnowflakeProvider>(s =>
                {
                    return ActivatorUtilities.CreateInstance<SnowflakeProvider>(s, 1u, 1u);
                });

            services.AddHsts(options =>
            {
                options.Preload = false;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromMinutes(30);

                // ensure we exclude a development machine
                options.ExcludedHosts.Add("localhost");
                options.ExcludedHosts.Add("127.0.0.1");
            });
        }

        /// <inheritdoc/>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
