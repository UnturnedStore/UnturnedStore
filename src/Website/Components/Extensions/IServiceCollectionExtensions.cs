using Website.Components.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Components.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddComponentsAndServices(this IServiceCollection services)
        {
            services.AddScoped<NavigationService>();
        }
    }
}
