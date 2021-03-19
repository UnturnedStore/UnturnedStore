using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Params;

namespace Website.Shared.Models
{
    public class OrderItemModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }

        public ProductModel Product { get; set; }
        public OrderModel Order { get; set; }

        public static OrderItemModel FromParams(OrderItemParams orderItemParams)
        {
            return new OrderItemModel()
            {
                ProductId = orderItemParams.ProductId
            };
        }
    }
}
