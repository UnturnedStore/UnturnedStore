using System;
using System.ComponentModel.DataAnnotations;

namespace Website.Shared.Models
{
    public class ProductCustomerModel
    {
        public int Id { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
        public DateTime CreateDate { get; set; }

        public UserModel User { get; set; }
        public ProductModel Product { get; set; }
    }
}
