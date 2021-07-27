using System;
using System.ComponentModel.DataAnnotations;

namespace Website.Shared.Models
{
    public class MProductCustomer
    {
        public int Id { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
        public DateTime CreateDate { get; set; }

        public MUser User { get; set; }
        public MProduct Product { get; set; }
    }
}
