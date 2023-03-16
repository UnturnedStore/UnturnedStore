using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Website.Shared.Models.Database;

namespace Website.Shared.Params
{
    public class OrderItemParams
    {
        public int ProductId { get; set; }
        public string CouponCode { get; set; }

        [JsonIgnore]
        public MProduct Product { get; set; }
        [JsonIgnore]
        public MProductCoupon Coupon { get; set; }
    }
}
