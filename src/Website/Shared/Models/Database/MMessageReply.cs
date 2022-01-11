using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models.Database
{
    public class MMessageReply
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int UserId { get; set; }
        [Required]
        [MaxLength(4000)]
        public string Content { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
