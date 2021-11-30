using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Payments.Abstractions;
using Website.Payments.Options;

namespace Website.Payments.Providers
{
    public class NanoPaymentProvider : IPaymentProvider
    {
        private readonly PaymentOptions options;
        private readonly OrdersRepository ordersRepository;

        public NanoPaymentProvider(IOptions<PaymentOptions> options, OrdersRepository ordersRepository)
        {
            this.options = options.Value;
            this.ordersRepository = ordersRepository;
        }

        public async Task CreateReceiveAccount()
        {

        }
    }
}
