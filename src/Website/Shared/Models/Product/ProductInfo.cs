using System;
using Website.Shared.Models.Database;

namespace Website.Shared.Models
{
    public class ProductInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string GithubUrl { get; set; }
        public int ImageId { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public bool IsLoaderEnabled { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime CreateDate { get; set; }

        public Seller Seller { get; set; }
        public MProductSale Sale { get; set; }

        public string GetShortDescription()
        {
            if (Description.Length > 100)
                return Description.Substring(0, 100).TrimEnd(' ') + "...";
            return Description;
        }

        public string GetPrice()
        {
            return Price == 0 ? "Free" : Price.ToString();
        }
    }
}
