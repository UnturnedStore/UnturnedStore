using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models.Database
{
    public class MMessageRead
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public int ReadId { get; set; }

        public MMessageRead() { }
        public MMessageRead(MMessage message, bool isRead)
        {
            MessageId = message.Id;
            UserId = isRead ? message.FromUserId : message.ToUserId;
            ReadId = isRead ? (message.Replies.Count == 0 ? 0 : message.Replies[message.Replies.Count - 1].Id) : -1;
        }
    }
}
