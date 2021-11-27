using CoinMarketCap;
using CoinMarketCap.Models.Cryptocurrency;
using Microsoft.Extensions.Options;
using NoobsMuc.Coinmarketcap.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Payments.Abstractions;
using Website.Payments.Constants;
using Website.Payments.Options;
using Website.Payments.Services;
using Website.Shared.Models;

namespace Website.Payments.Providers
{
    public class NanoPaymentProvider : IPaymentProvider
    {
        private readonly PaymentOptions options;
        private readonly OrdersRepository ordersRepository;
        private readonly NanoPaymentStore paymentStore;

        public NanoPaymentProvider(IOptions<PaymentOptions> options, OrdersRepository ordersRepository, NanoPaymentStore paymentStore)
        {
            this.options = options.Value;
            this.ordersRepository = ordersRepository;
            this.paymentStore = paymentStore;
        }

        public void BuildOrder(MOrder order)
        {
            order.PaymentReceiver = order.Seller.NanoAddress;
            order.Currency = "USD";
        }

        public async Task<MNanoPayment> CreatePaymentAsync(MOrder order)
        {
            CoinMarketCapClient client = new CoinMarketCapClient(options.Providers.Nano.CoinMarketCapAPIKey);
            CoinMarketCap.Models.Response<List<Metadata>> info = client.GetCryptocurrencyInfo(new MetadataParameters()
            {
                Symbols = new string[]
                {
                    "Nano"
                }
            });


            IEnumerable<Currency> currencies =  client.GetCurrencyBySymbolList(new string[] { "usd" }, "nano");
            
            await paymentStore.AddNanoPaymentAsync();
        }
    }
}
