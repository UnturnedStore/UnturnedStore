using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Enums;

namespace Website.Shared.Models
{
    public class PrivateProduct : ProductInfo
    {
        public int? AdminId { get; set; }
        public ProductStatus Status { get; set; }
        public string StatusReason { get; set; }
    }
}
