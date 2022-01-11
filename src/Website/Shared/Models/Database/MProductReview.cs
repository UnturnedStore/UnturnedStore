using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models.Database
{
    public class MProductReview
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        [Required]
        [MaxLength(2000)]
        public string Body { get; set; }
        [Required]
        [Range(1, 5)]
        public byte Rating { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime CreateDate { get; set; }

        public MUser User { get; set; }

        public static MProductReview FromReview(MProductReview review)
        {
            return new MProductReview()
            {
                Id = review.Id,
                Title = review.Title,
                Body = review.Body,
                Rating = review.Rating,
                ProductId = review.ProductId,
                UserId = review.UserId,
                LastUpdate = review.LastUpdate,
                CreateDate = review.CreateDate,
                User = review.User
            };
        }
    }
}
