using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Name { get; set; }
        public string Role { get; set; }
        public string SteamId { get; set; }
        public byte[] Avatar { get; set; }
        [MaxLength(255)]
        public string PayPalEmail { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(3)]
        public string PayPalCurrency { get; set; }
        [MaxLength(255)]
        public string DiscordWebhookUrl { get; set; }
        public DateTime CreateDate { get; set; }

        public List<ProductCustomerModel> Products { get; set; }
    }
}
