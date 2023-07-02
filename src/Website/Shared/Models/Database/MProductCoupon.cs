using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Website.Shared.Enums;

namespace Website.Shared.Models.Database
{
    public class MProductCoupon
    {
        public int Id { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(30)]
        public string CouponName { get; set; }
        [Required]
        [MaxLength(16)]
        public string CouponCode { get; set; }
        [Required]
        [Range(0.01, 0.99)]
        public decimal CouponMultiplier { get; set; }
        public int? MaxUses { get; set; }
        public bool IsEnabled { get; set; }

        public MProduct Product { get; set; }
        public int? CouponUsageCount { get; set; }

        public MProductCoupon() { }

        public MProductCoupon(MProductCoupon coupon)
        {
            Id = coupon.Id;
            ProductId = coupon.ProductId;
            CouponName = coupon.CouponName;
            CouponCode = coupon.CouponCode;
            CouponMultiplier = coupon.CouponMultiplier;
            MaxUses = coupon.MaxUses;
            IsEnabled = coupon.IsEnabled;
            Product = coupon.Product;
            CouponUsageCount = coupon.CouponUsageCount;
        }
    }
}
