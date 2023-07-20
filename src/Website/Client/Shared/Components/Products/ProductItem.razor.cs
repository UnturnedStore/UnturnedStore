using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Website.Shared.Models.Database;

namespace Website.Client.Shared.Components.Products
{
    public partial class ProductItem
    {
        [Parameter]
        public MProduct Product { get; set; }

        [Parameter]
        public List<MProductTag> ProductTags { get; set; }

        public string GetClass()
        {
            if (Product.IsLoaderEnabled)
            {
                return "border-loader";
            }

            return string.Empty;
        }
    }
}
