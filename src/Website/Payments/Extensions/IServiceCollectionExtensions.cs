using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Website.Payments.Abstractions;
using Website.Payments.Options;
using Website.Payments.Providers;

namespace Website.Payments.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddPayments(this IServiceCollection services)
        {
            services.AddTransient<IPaymentProvider, NanoPaymentProvider>();
            services.AddTransient<IPaymentProvider, PayPalPaymentProvider>();
            services.AddTransient<IPaymentProviders, PaymentProviders>((provider) => new PaymentProviders(provider.GetServices<IPaymentProvider>()));

            
        }
    }
}
