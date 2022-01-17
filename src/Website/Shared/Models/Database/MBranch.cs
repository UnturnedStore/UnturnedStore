using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models.Database
{
    public class MBranch
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

        public MProduct Product { get; set; }
        public List<MVersion> Versions { get; set; }

        public static MBranch FromBranch(MBranch branch)
        {
            return new MBranch()
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
