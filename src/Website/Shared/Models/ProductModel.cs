using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Website.Shared.Models
{
    public class ProductModel
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
        public int ImageId { get; set; }
        [Range(0, 1000)]
        public decimal Price { get; set; }
        public int SellerId { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime CreateDate { get; set; }

        public int TotalDownloadsCount { get; set; }

        public UserModel Seller { get; set; }
        public UserModel Customer { get; set; }

        public List<ProductTabModel> Tabs { get; set; }
        public List<ProductMediaModel> Medias { get; set; }

        public List<BranchModel> Branches { get; set; }

        public string GetDescription()
        {
            if (Description.Length > 100)
                return Description.Substring(0, 100).TrimEnd(' ') + "...";
            return Description;
        }

        public static ProductModel FromProduct(ProductModel product)
        {
            return new ProductModel()
            {
                Id = product.Id,
                Price = product.Price,
                Description = product.Description,
                GithubUrl = product.GithubUrl,
                Name = product.Name,
                ImageId = product.ImageId,
                IsEnabled = product.IsEnabled,
                LastUpdate = product.LastUpdate,
                SellerId = product.SellerId,
                CreateDate = product.CreateDate
            };
        }
    }
}
