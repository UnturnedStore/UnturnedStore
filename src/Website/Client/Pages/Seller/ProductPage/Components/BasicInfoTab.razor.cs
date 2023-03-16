using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Website.Shared.Models.Database;
using Website.Shared.Models;
using Website.Components.Alerts;
using System.Net;
using System.Collections.Generic;
using Website.Shared.Constants;

namespace Website.Client.Pages.Seller.ProductPage.Components
{
    public partial class BasicInfoTab
    {
        [Parameter]
        public SellerProduct Product { get; set; }

        [Parameter]
        public EventCallback<SellerProduct> ProductChanged { get; set; }

        public List<MProductTag> ProductTags { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            ProductTags = await HttpClient.GetFromJsonAsync<List<MProductTag>>("api/products/tags");
        }

        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            var imageFile = e.File;
            if (e.File.ContentType != "image/png")
            {
                AlertService.ShowAlert("product-basicinfo", "Product image must be in <strong>png</strong> format!", AlertType.Danger);
                return;
            }
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
            HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/products", Product.ToMProduct());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                await ProductChanged.InvokeAsync(Product);
                AlertService.ShowAlert("product-basicinfo", "Successfully updated your product!", AlertType.Success);
            } else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                AlertService.ShowAlert("product-basicinfo", $"Product name <strong>{Product.Name}</strong> is already taken!", AlertType.Danger);
            } else
            {
                AlertService.ShowAlert("product-basicinfo", "There was an error when trying to update your product!", AlertType.Danger);
            }
            
            isLoading = false;
        }
    }
}