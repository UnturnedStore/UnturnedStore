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
    public class MProductSale
    {
        public int Id { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        public decimal ProductPrice { get; set; }

        [Required]
        [MaxLength(30)]
        public string SaleName { get; set; }
        [Required]
        [Range(0.01, 0.99)]
        public decimal SaleMultiplier { get; set; }
        public DateTime? StartDate { get; set; }
        [JsonIgnore]
        public DateTime SaleStart => StartDate ?? DateTime.Now ;
        public bool IsActive { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public bool IsExpired { get; set; }

        [JsonIgnore]
        public ProductSaleStatus Status
        {
            get
            {
                if (IsActive) return ProductSaleStatus.Active;
                else if (IsExpired) return ProductSaleStatus.Expired;
                else return ProductSaleStatus.Upcoming;
            }
        }

        [JsonIgnore]
        public MProduct Product { get; set; }
        public int? SaleUsageCount { get; set; }

        public MProductSale() { }

        public MProductSale(MProductSale sale)
        {
            Id = sale.Id;
            ProductId = sale.ProductId;
            ProductPrice = sale.ProductPrice;
            SaleName = sale.SaleName;
            SaleMultiplier = sale.SaleMultiplier;
            StartDate = sale.StartDate;
            EndDate = sale.EndDate;
            IsExpired = sale.IsExpired;
            Product = sale.Product;
        }
    }
}
