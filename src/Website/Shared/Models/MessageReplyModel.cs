using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models
{
    public class MessageReplyModel
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
