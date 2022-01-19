using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Website.Client.Extensions;
using Website.Shared.Models.Database;
using Website.Shared.Constants;

namespace Website.Client.Pages.Seller.ProductsPage.Components
{
    public partial class CreateProductModal
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        [Parameter]
        public EventCallback<MProduct> OnSubmitAsync { get; set; }

        public MProduct Model { get; set; } = new MProduct() { Category = ProductConstants.DefaultCategory };
        public async Task ShowAsync()
        {
            await JSRuntime.ShowModalStaticAsync(nameof(CreateProductModal));
        }

        private bool isLoading = false;
        public async Task SubmitAsync()
        {
            isLoading = true;
            await OnSubmitAsync.InvokeAsync(Model);
            isLoading = false;
            await JSRuntime.HideModalAsync(nameof(CreateProductModal));
            Model = new MProduct() { Category = ProductConstants.DefaultCategory };
        }

        private void OnPriceInput(ChangeEventArgs args)
        {
            string val = args.Value.ToString();
            if (string.IsNullOrEmpty(val))
                Model.Price = 0;
            else
                Model.Price = decimal.Parse(args.Value.ToString());

            if (Model.Price == 0)
            {
                Model.IsLoaderEnabled = false;
            }
        }

        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            var imageFile = e.File;
            imageFile = await imageFile.RequestImageFileAsync("image/png", 500, 500);
            var image = new MImage
            {
                ContentType = imageFile.ContentType,
                Name = imageFile.Name,
                Content = new byte[imageFile.Size]
            };
            await imageFile.OpenReadStream(50 * 1024 * 1024).ReadAsync(image.Content);
            var response = await HttpClient.PostAsJsonAsync("api/images", image);
            Model.ImageId = int.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}