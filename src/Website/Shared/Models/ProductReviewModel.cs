using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models
{
    public class ProductReviewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public byte Rating { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime CreateDate { get; set; }

        public UserModel User { get; set; }
    }
}
