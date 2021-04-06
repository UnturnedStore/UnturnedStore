using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Client.Pages.Admin
{
    [Authorize(Roles = RoleConstants.AdminRoleId)]
    public partial class ProductsAdminPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public IEnumerable<ProductModel> Products { get; set; }
        private ICollection<ProductModel> orderedProducts => Products.Where(x => string.IsNullOrEmpty(searchString)
            || x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
            || x.Seller.Name.Equals(searchString, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreateDate).ToList();

        private string searchString = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            Products = await HttpClient.GetFromJsonAsync<ProductModel[]>("api/admin/products");
        }

        private void GoToProduct(ProductModel product)
        {
            NavigationManager.NavigateTo("/seller/products/" + product.Id);
        }
    }
}
