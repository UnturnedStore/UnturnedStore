using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Website.Client.Providers;
using Website.Client.Services;
using Website.Components.Extensions;

namespace Website.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, SteamAuthProvider>();
            builder.Services.AddScoped<CartService>();
            builder.Services.AddTransient<StorageService>();
            builder.Services.AddTransient<ZIPService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<MessageReadService>();
            builder.Services.AddComponentsAndServices();

            WebAssemblyHost host = builder.Build();
            await host.Services.GetRequiredService<CartService>().ReloadCartAsync();
            await host.RunAsync();
        }
    }
}
