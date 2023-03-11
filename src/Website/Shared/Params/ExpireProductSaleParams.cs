using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models.Database;

namespace Website.Shared.Params
{
    public class ExpireProductSaleParams
    {
        public int ProductSaleId { get; set; }
        public decimal ProductPrice { get; set; }

        public ExpireProductSaleParams() { }
        public ExpireProductSaleParams(MProductSale Sale)
        {
            ProductSaleId = Sale.Id;
            ProductPrice = Sale.Product.Price;
        }
    }
}
