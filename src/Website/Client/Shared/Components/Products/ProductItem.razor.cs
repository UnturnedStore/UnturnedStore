using Microsoft.AspNetCore.Components;
using Website.Shared.Models.Database;

namespace Website.Client.Shared.Components.Products
{
    public partial class ProductItem
    {
        [Parameter]
        public MProduct Product { get; set; }
    }
}
