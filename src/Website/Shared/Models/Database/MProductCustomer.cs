using System;
using System.ComponentModel.DataAnnotations;

namespace Website.Shared.Models.Database
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
        public Guid LicenseKey { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? BlockDate { get; set; }
        public DateTime CreateDate { get; set; }

        public UserInfo User { get; set; }
        public MProduct Product { get; set; }
    }
}
