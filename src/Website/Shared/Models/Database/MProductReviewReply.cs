using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models.Database
{
    public class MProductReviewReply
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Body { get; set; }
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime CreateDate { get; set; }

        public UserInfo User { get; set; }

        public MProductReviewReply() { }
        public MProductReviewReply(MProductReviewReply reviewReply)
        {
            Id = reviewReply.Id;
            Body = reviewReply.Body;
            ReviewId = reviewReply.ReviewId;
            UserId = reviewReply.UserId;
            LastUpdate = reviewReply.LastUpdate;
            CreateDate = reviewReply.CreateDate;
            User = reviewReply.User;
        }
    }
}
