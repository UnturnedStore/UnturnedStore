using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        public bool IsClosed { get; set; }
        public int ClosingUserId { get; set; }
        public DateTime CreateDate { get; set; }        

        public UserModel FromUser { get; set; }
        public UserModel ToUser { get; set; }

        public List<MessageReplyModel> Replies { get; set; }
    }
}
