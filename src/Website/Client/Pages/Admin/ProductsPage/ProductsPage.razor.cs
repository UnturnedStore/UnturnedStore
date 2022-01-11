using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Admin.ProductsPage
{
    [Authorize(Roles = RoleConstants.AdminRoleId)]
    public partial class ProductsPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public IEnumerable<MProduct> Products { get; set; }
        private ICollection<MProduct> orderedProducts => Products.Where(x => string.IsNullOrEmpty(searchString)
            || x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
            || x.Seller.Name.Equals(searchString, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreateDate).ToList();

        private string searchString = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            Products = await HttpClient.GetFromJsonAsync<MProduct[]>("api/admin/products");
        }

        private void GoToProduct(MProduct product)
        {
            NavigationManager.NavigateTo("/seller/products/" + product.Id);
        }
    }
}
