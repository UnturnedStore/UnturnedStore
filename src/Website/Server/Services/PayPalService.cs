using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Server.Services
{
    public class PayPalService
    {
        private readonly ProductsRepository productsRepository;
        private readonly OrdersRepository ordersRepository;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<PayPalService> logger;
        private readonly OrderService orderService;

        public PayPalService(ProductsRepository productsRepository, OrdersRepository ordersRepository, IHttpClientFactory httpClientFactory, 
            ILogger<PayPalService> logger, OrderService orderService)
        {
            this.productsRepository = productsRepository;
            this.ordersRepository = ordersRepository;
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.orderService = orderService;
        }

        public static void PayPalPayment(OrderModel order, string baseUrl)
        {
            var dict = new Dictionary<string, string>()
            {
                { "cmd", "_cart" },
                { "upload", "1" },
                { "business", order.PaymentReceiver },
                { "custom", order.Id.ToString() },
                { "currency_code", order.Currency },
                { "no_shipping", "1" },
                { "no_note", "1" },
                { "notify_url", baseUrl + "/api/orders/" + PaymentContants.PayPal },
                { "return", baseUrl + "/orders" },
                { "cancel_return", baseUrl }
            };

            for (int i = 1; i <= order.Items.Count; i++)
            {
                var item = order.Items[i - 1];
                dict.Add("item_name_" + i, item.ProductName);
                dict.Add("item_number_" + i, item.ProductId.ToString());
                dict.Add("amount_" + i, item.Price.ToString());
            }

            order.PaymentUrl = QueryHelpers.AddQueryString(PaymentContants.PayPalUrl, dict);
        }

        public async Task ProcessPaymentAsync(HttpRequest request)
        {
            string requestBody;
            using (var reader = new StreamReader(request.Body, Encoding.ASCII))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            var content = new StringContent("cmd=_notify-validate&" + requestBody);

            var response = await httpClientFactory.CreateClient().PostAsync(PaymentContants.PayPalUrl, content);
            var verification = await response.Content.ReadAsStringAsync();

            if (!verification.Equals("VERIFIED"))
            {
                return;
            }

            var dict = HttpUtility.ParseQueryString(requestBody);
            
            if (!int.TryParse(dict["custom"], out int orderId))
            {
                throw new Exception($"Invalid payment custom: {dict["custom"]}");
            }

            var order = await ordersRepository.GetOrderAsync(orderId);

            if (dict["receiver_email"] != order.PaymentReceiver)
            {
                logger.LogWarning($"Payment receive email ");
                return;
            }

            if (decimal.Parse(dict["mc_gross"]) < order.TotalPrice || dict["mc_currency"] != order.Currency)
            {
                logger.LogWarning($"Payment price or currency doesn't match order {dict["custom"]}");
                return;
            }

            order.TransactionId = dict["txn_id"];
            order.Status = dict["payment_status"];
            order.PaymentPayer = dict["payer_email"];

            await ordersRepository.UpdateOrderAsync(order);

            if (order.Status == "Completed")
            {
                await orderService.CompleteOrderAsync(order);
            }
        }

    }
}
