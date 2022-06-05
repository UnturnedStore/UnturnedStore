using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models.Database
{
    public class MMessage
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

        public UserInfo FromUser { get; set; }
        public UserInfo ToUser { get; set; }

        public List<MMessageReply> Replies { get; set; }
        public MMessageRead Read { get; set; }
    }
}
