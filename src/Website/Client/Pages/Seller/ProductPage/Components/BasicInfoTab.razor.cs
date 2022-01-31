using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.ProductPage.Components
{
    public partial class BasicInfoTab
    {
        [Parameter]
        public MProduct Product { get; set; }

        [Parameter]
        public EventCallback<MProduct> ProductChanged { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            var imageFile = e.File;
            imageFile = await imageFile.RequestImageFileAsync("image/png", 500, 500);
            var image = new MImage();
            image.ContentType = imageFile.ContentType;
            image.Name = imageFile.Name;
            image.Content = new byte[imageFile.Size];
            await imageFile.OpenReadStream(50 * 1024 * 1024).ReadAsync(image.Content);
            var response = await HttpClient.PostAsJsonAsync("api/images", image);
            Product.ImageId = int.Parse(await response.Content.ReadAsStringAsync());
        }

        private bool IsDisabled => Product.IsLoaderEnabled && Product.Price == 0;

        private bool isLoading;
        private async Task SubmitAsync()
        {
            isLoading = true;
            await HttpClient.PutAsJsonAsync($"api/products", MProduct.FromProduct(Product));
            await ProductChanged.InvokeAsync(Product);
            isLoading = false;
        }
    }
}