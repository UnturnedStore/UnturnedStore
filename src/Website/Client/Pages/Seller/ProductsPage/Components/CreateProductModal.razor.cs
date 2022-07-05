using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Website.Client.Extensions;
using Website.Shared.Models.Database;
using Website.Shared.Constants;
using Website.Components.Alerts;
using System.Net;
using System.Collections.Generic;

namespace Website.Client.Pages.Seller.ProductsPage.Components
{
    public partial class CreateProductModal
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }

        [Parameter]
        public EventCallback<MProduct> OnProductAdded { get; set; }

        public MProduct Model { get; set; } = new MProduct() { Category = ProductCategoryConstants.DefaultCategory, Tags = new List<MProductTag>() };
        public List<MProductTag> ProductTags { get; set; }

        public async Task ShowAsync()
        {
            await JSRuntime.ShowModalStaticAsync(nameof(CreateProductModal));
            ProductTags = await HttpClient.GetFromJsonAsync<List<MProductTag>>("api/products/tags");
        }

        private bool isLoading = false;
        public async Task SubmitAsync()
        {
            isLoading = true;

            HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/products", Model);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    AlertService.ShowAlert("products-modal-create", $"There's already a product with this name", AlertType.Danger);
                } else
                {
                    AlertService.ShowAlert("products-modal-create", $"An error occurated <strong>{response.StatusCode}</strong>", AlertType.Danger);
                }
            } else
            {
                MProduct product = await response.Content.ReadFromJsonAsync<MProduct>();
                await OnProductAdded.InvokeAsync(product);

                AlertService.ShowAlert("products-main", $"Successfully created new product <strong>{product.Name}</strong>!", AlertType.Success);
                
                await JSRuntime.HideModalAsync(nameof(CreateProductModal));
                Model = new MProduct() { Category = ProductCategoryConstants.DefaultCategory, Tags = new List<MProductTag>() };
                StateHasChanged();
            }

            isLoading = false;
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
            IBrowserFile imageFile = e.File;

            if (e.File.ContentType != "image/png")
            {
                AlertService.ShowAlert("products-modal-image", "Product image must be in <strong>png</strong> format!", AlertType.Danger);
                return;
            }

            imageFile = await imageFile.RequestImageFileAsync("image/png", 500, 500);
            MImage image = new MImage
            {
                ContentType = imageFile.ContentType,
                Name = imageFile.Name,
                Content = new byte[imageFile.Size]
            };

            await imageFile.OpenReadStream(50 * 1024 * 1024).ReadAsync(image.Content);
            
            HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/images", image);
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                AlertService.ShowAlert("products-modal-image", $"An error occurated <strong>{response.StatusCode}</strong>", AlertType.Danger);
                return;
            }

            string imageIdStr = await response.Content.ReadAsStringAsync();
            Model.ImageId = int.Parse(imageIdStr);
        }
    }
}