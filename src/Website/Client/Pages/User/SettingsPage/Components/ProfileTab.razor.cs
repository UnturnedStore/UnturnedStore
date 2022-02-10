using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.User.SettingsPage.Components
{
    public partial class ProfileTab
    {
        [Parameter]
        public MUser User { get; set; }
        [Parameter]
        public bool IsLoading { get; set; }
        [Parameter]
        public EventCallback<bool> IsLoadingChanged { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private async Task SubmitAsync()
        {
            IsLoading = true;
            await IsLoadingChanged.InvokeAsync(IsLoading);
            var user = MUser.FromUser(User);

            if (avatarPreview != null)
            {
                var response = await HttpClient.PostAsJsonAsync("api/images", avatarPreview);
                user.AvatarImageId = int.Parse(await response.Content.ReadAsStringAsync());
            }
            
            await HttpClient.PutAsJsonAsync("api/users/profile", user);
            NavigationManager.NavigateTo(NavigationManager.Uri, true);
            IsLoading = false;
        }

        private MImage avatarPreview = null;
        private string avatarPreviewSrc => $"data:{avatarPreview.ContentType};base64,{Convert.ToBase64String(avatarPreview.Content)}";

        private async Task OnAvatarInputFileChange(InputFileChangeEventArgs e)
        {
            var imageFile = await e.File.RequestImageFileAsync(e.File.ContentType, 500, 500);

            avatarPreview = new MImage
            {
                ContentType = imageFile.ContentType,
                Name = imageFile.Name,
                Content = new byte[imageFile.Size]
            };
            await imageFile.OpenReadStream(2 * 1024 * 1024).ReadAsync(avatarPreview.Content);
        }
    }
}
