using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Website.Shared.Models.Database;
using Website.Shared.Models;

namespace Website.Client.Pages.Seller.ProductPage.Components
{
    public partial class MediaTab
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        [Parameter]
        public SellerProduct Product { get; set; }

        private string YoutubeUrl;
        private async Task AddYoutubeUrl()
        {
            var media = new MProductMedia()
            {ProductId = Product.Id, YoutubeUrl = YoutubeUrl};
            await AddMediaAsync(media);
            YoutubeUrl = null;
        }

        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            var imageFile = e.File;
            var image = new MImage();
            image.ContentType = imageFile.ContentType;
            image.Name = imageFile.Name;
            image.Content = new byte[imageFile.Size];
            await imageFile.OpenReadStream(50 * 1024 * 1024).ReadAsync(image.Content);
            var response = await HttpClient.PostAsJsonAsync("api/images", image);
            var media = new MProductMedia()
            {ProductId = Product.Id, ImageId = int.Parse(await response.Content.ReadAsStringAsync())};
            await AddMediaAsync(media);
        }

        private async Task AddMediaAsync(MProductMedia media)
        {
            var response = await HttpClient.PostAsJsonAsync("api/products/medias", media);
            Product.Media.Add(await response.Content.ReadFromJsonAsync<MProductMedia>());
        }

        private async Task DeleteAsync(MProductMedia media)
        {
            await HttpClient.DeleteAsync("api/products/medias/" + media.Id);
            Product.Media.Remove(media);
        }
    }
}