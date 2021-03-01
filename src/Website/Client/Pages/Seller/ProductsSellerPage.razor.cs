using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Pages.Seller.Components;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Client.Pages.Seller
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class ProductsSellerPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public List<ProductModel> Products { get; set; }

        public CreateProductModal Modal { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Products = await HttpClient.GetFromJsonAsync<List<ProductModel>>("api/seller/products");
        }

        private async Task PostProductAsync(ProductModel product)
        {
            var response = await HttpClient.PostAsJsonAsync("api/products", product);
            Products.Add(await  response.Content.ReadFromJsonAsync<ProductModel>());
        }

        public async Task ShowModalAsync()
        {
            await Modal.ShowAsync();
        }

        private void GoToProduct(ProductModel product)
        {
            NavigationManager.NavigateTo($"/seller/products/{product.Id}");
        }
    }
}
