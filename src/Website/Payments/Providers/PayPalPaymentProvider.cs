using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Website.Data.Repositories;
using Website.Payments.Abstractions;
using Website.Payments.Constants;
using Website.Payments.Models;
using Website.Payments.Options;
using Website.Shared.Models;

namespace Website.Payments.Providers
{
    public class PayPalPaymentProvider : IPaymentProvider
    {
        private readonly PaymentOptions options;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly OrdersRepository ordersRepository;

        public PayPalPaymentProvider(IOptions<PaymentOptions> options, IHttpClientFactory httpClientFactory, OrdersRepository ordersRepository)
        {
            this.options = options.Value;
            this.httpClientFactory = httpClientFactory;
            this.ordersRepository = ordersRepository;
        }

        public string PayPalUrl => PaymentConstants.Providers.PayPal.GetPayPalUrl(options.Providers.PayPal.UseSandbox);

        public void BuildOrder(MOrder order)
        {
            order.PaymentReceiver = order.Seller.PayPalAddress;
            order.Currency = order.Seller.PayPalCurrency;
        }

        public string GetPaymentUrl(MOrder order, string baseUrl)
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
                { "notify_url", PaymentConstants.Providers.PayPal.GetNotifyUrl(baseUrl) },
                { "return", baseUrl },
                { "cancel_return", baseUrl },
                { "image_url", PaymentConstants.GetLogoUrl(baseUrl) }
            };

            for (int i = 1; i <= order.Items.Count; i++)
            {
                var item = order.Items[i - 1];
                dict.Add("item_name_" + i, item.ProductName);
                dict.Add("item_number_" + i, item.ProductId.ToString());
                dict.Add("amount_" + i, item.Price.ToString());
            }

            return QueryHelpers.AddQueryString(PayPalUrl, dict);
        }

        public async Task<ValidatePaymentResult> ValidatePaymentAsync(string requestBody)
        {
            ValidatePaymentResult result = new();

            StringContent content = new("cmd=_notify-validate&" + requestBody);

            HttpResponseMessage response = await httpClientFactory.CreateClient().PostAsync(PayPalUrl, content);
            string verification = await response.Content.ReadAsStringAsync();

            if (!verification.Equals("VERIFIED"))
            {
                result.Status = PaymentStatus.NotVerified;
                result.ErrorMessage = "The PayPal payment couldn't be verified";
                return result;
            }

            NameValueCollection dict = HttpUtility.ParseQueryString(requestBody);

            if (!int.TryParse(dict["custom"], out int orderId))
            {
                result.Status = PaymentStatus.InvalidCustom;
                result.ErrorMessage = $"The PayPal payment has invalid custom parameter: {dict["custom"]}";
                return result;
            }

            result.Order = await ordersRepository.GetOrderAsync(orderId);

            if (dict["receiver_email"] != result.Order.PaymentReceiver)
            {
                result.Status = PaymentStatus.ReceiverMismatch;
                result.ErrorMessage = $"The PayPal payment receiver email doesn't match the order: {dict["receiver_email"]}";
                return result;
            }

            if (dict["mc_currency"] != result.Order.Currency)
            {
                result.Status = PaymentStatus.CurrencyMismatch;
                result.ErrorMessage = $"The PayPal payment currency doesn't match the order: {dict["mc_currency"]}";
                return result;
            }

            if (decimal.Parse(dict["mc_gross"]) < result.Order.TotalPrice)
            {
                result.Status = PaymentStatus.AmountMismatch;
                result.ErrorMessage = $"The PayPal payment gross doesn't match the order value: {dict["mc_gross"]}";
                return result;
            }

            result.Status = PaymentStatus.Valid;
            result.Order.TransactionId = dict["txn_id"];
            result.Order.Status = dict["payment_status"];
            result.Order.PaymentSender = dict["payer_email"];

            return result;
        }
    }
}
