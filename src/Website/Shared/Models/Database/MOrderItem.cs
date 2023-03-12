using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Params;

namespace Website.Shared.Models.Database
{
    public class MOrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int? SaleId { get; set; }
        public int? CouponId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal? CouponMultiplier { get; set; }

        public MProduct Product { get; set; }
        public string CouponCode { get; set; }
        public MProductCoupon Coupon { get; set; }
        public MOrder Order { get; set; }

        public static MOrderItem FromParams(OrderItemParams orderItemParams)
        {
            return new MOrderItem()
            {
                ProductId = orderItemParams.ProductId,
                CouponCode = orderItemParams.CouponCode
            };
        }
    }
}
