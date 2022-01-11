using RestoreMonarchy.PaymentGateway.Client.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Params;

namespace Website.Shared.Models.Database
{
    public class MOrder
    {
        public int Id { get; set; }
        public Guid PaymentId { get; set; }
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentReceiver { get; set; }
        public string PaymentSender { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime CreateDate { get; set; }

        public UserInfo Buyer { get; set; }
        public Seller Seller { get; set; }
        public List<MOrderItem> Items { get; set; }

        public string GetReceiver(string paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentProviders.PayPal => Seller.PayPalAddress,
                PaymentProviders.Nano => Seller.NanoAddress,
                PaymentProviders.Mock => Seller.Name,
                _ => null,
            };
        }

        public static MOrder FromParams(OrderParams orderParams)
        {
            return new MOrder()
            {
                BuyerId = orderParams.BuyerId,
                SellerId = orderParams.SellerId,
                PaymentMethod = orderParams.PaymentMethod,
                Items = new List<MOrderItem>()
            };
        }
    }
}
