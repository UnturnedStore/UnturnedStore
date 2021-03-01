using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models
{
    public class ProductCustomerModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public DateTime CreateDate { get; set; }

        public UserModel User { get; set; }
        public ProductModel Product { get; set; }
    }
}
