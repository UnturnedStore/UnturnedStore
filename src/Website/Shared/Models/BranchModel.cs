using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models
{
    public class BranchModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        [MaxLength(255)]
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreateDate { get; set; }

        public ProductModel Product { get; set; }
        public List<VersionModel> Versions { get; set; }

        public static BranchModel FromBranch(BranchModel branch)
        {
            return new BranchModel()
            {
                Id = branch.Id,
                ProductId = branch.ProductId,
                Description = branch.Description,
                Name = branch.Name,
                IsEnabled = branch.IsEnabled,
                CreateDate = branch.CreateDate
            };
        }
    }
}
