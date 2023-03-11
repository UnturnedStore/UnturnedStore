using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Website.Shared.Enums;

namespace Website.Shared.Models.Database
{
    public class MProduct
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }
        [Required]
        [MaxLength(255)]
        public string Description { get; set; }
        [MaxLength(255)]
        public string GithubUrl { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int ImageId { get; set; }
        [Range(0, 1000)]
        public decimal Price { get; set; }
        [Required]
        public string Category { get; set; }
        public int SellerId { get; set; }
        public int? AdminId { get; set; }
        public ProductStatus Status { get; set; }
        public string StatusReason { get; set; }
        public DateTime StatusUpdateDate { get; set; }
        public bool IsLoaderEnabled { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime CreateDate { get; set; }

        public int TotalDownloadsCount { get; set; }
        public byte AverageRating { get; set; }
        public int RatingsCount { get; set; }
        public int ServersCount { get; set; }

        public Seller Seller { get; set; }
        public UserInfo Customer { get; set; }
        public MProductSale Sale { get; set; }

        public List<MProductTab> Tabs { get; set; }
        public List<MProductMedia> Medias { get; set; }
        public List<MProductReview> Reviews { get; set; }
        public List<MProductWorkshopItem> WorkshopItems { get; set; }
        public List<MBranch> Branches { get; set; }
        public List<MProductTag> Tags { get; set; }

        public string GetDescription()
        {
            if (Description.Length > 100)
                return Description.Substring(0, 100).TrimEnd(' ') + "...";
            return Description;
        }

        public decimal DiscountedPrice()
        {
            if (Sale == null) return Price;
            else return Math.Round(Price * Sale.SaleMultiplier, 2);
        }

        public decimal DiscountedPrice(MProductCoupon coupon)
        {
            if (Sale == null && coupon == null) return Price;
            else if (Sale != null && coupon == null) return Math.Round(Price * Sale.SaleMultiplier, 2);
            else if (Sale == null && coupon != null) return Math.Round(Price * coupon.CouponMultiplier, 2);
            else return Math.Round(Math.Round(Price * Sale.SaleMultiplier, 2) * coupon.CouponMultiplier, 2);
        }
    }
}
