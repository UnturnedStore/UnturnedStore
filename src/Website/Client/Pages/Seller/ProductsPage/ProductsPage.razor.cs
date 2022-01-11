using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Pages.Seller.ProductsPage.Components;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.ProductsPage
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class ProductsPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public List<MProduct> Products { get; set; }

        public CreateProductModal Modal { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Products = await HttpClient.GetFromJsonAsync<List<MProduct>>("api/seller/products");
        }

        private async Task PostProductAsync(MProduct product)
        {
            var response = await HttpClient.PostAsJsonAsync("api/products", product);
            Products.Add(await  response.Content.ReadFromJsonAsync<MProduct>());
        }

        public async Task ShowModalAsync()
        {
            await Modal.ShowAsync();
        }

        private void GoToProduct(MProduct product)
        {
            NavigationManager.NavigateTo($"/seller/products/{product.Id}");
        }
    }
}
