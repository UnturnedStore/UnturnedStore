using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RestoreMonarchy.PaymentGateway.Client;
using SteamWebAPI2.Utilities;
using System;
using System.Data.SqlClient;
using System.IO;
using Website.Data.Extensions;
using Website.Server.Helpers;
using Website.Server.Options;
using Website.Server.Services;

namespace Website.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(x => new SqlConnection(Configuration.GetConnectionString("Default")));
            services.AddRepositories();

            services.AddTransient<OrderService>();
            services.AddTransient<DiscordService>();
            services.AddTransient<IBaseUrl, BaseUrlService>();

            services.AddOptions();
            services.Configure<PaymentOptions>(Configuration.GetSection(PaymentOptions.Key));

            services.AddTransient<PaymentGatewayClient>((s) => new (s.GetRequiredService<IOptions<PaymentOptions>>().Value));

            services.AddTransient(x => new SteamWebInterfaceFactory(Configuration["SteamWebAPIKey"]));           

            services.AddAuthentication(options => options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/signin";
                    options.LogoutPath = "/signout";
                    options.AccessDeniedPath = "/";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.Events.OnSignedIn = ValidationHelper.SignIn;
                    options.Events.OnValidatePrincipal = ValidationHelper.Validate;
                })
                .AddSteam();

            services.AddHttpClient();

            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimit"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddHttpContextAccessor();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseIpRateLimiting();

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseRouting();

            app.UseCookiePolicy();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
