using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Data.Repositories;

namespace Website.Data.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<UsersRepository>();
            services.AddScoped<ImagesRepository>();
            services.AddScoped<ProductsRepository>();
            services.AddScoped<BranchesRepository>();
            services.AddScoped<VersionsRepository>();
            services.AddScoped<SellersRepository>();
            services.AddScoped<OrdersRepository>();
            services.AddScoped<MessagesRepository>();
            services.AddScoped<AdminRepository>();
            services.AddScoped<PaymentsRepository>();
        }
    }
}
