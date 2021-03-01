using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Params;

namespace Website.Shared.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentReceiver { get; set; }
        public string PaymentUrl { get; set; }
        public string PaymentPayer { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime CreateDate { get; set; }

        public UserModel Buyer { get; set; }
        public UserModel Seller { get; set; }
        public List<OrderItemModel> Items { get; set; }

        public static OrderModel FromParams(OrderParams orderParams)
        {
            return new OrderModel()
            {
                BuyerId = orderParams.BuyerId,
                SellerId = orderParams.SellerId,
                PaymentMethod = orderParams.PaymentMethod,
                Items = new List<OrderItemModel>()
            };
        }
    }
}
