using Blazored.TextEditor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Providers;
using Website.Shared.Models;

namespace Website.Client.Pages.User.SettingsPage
{
    [Authorize]
    public partial class SettingsPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public MUser User { get; set; }

        private BlazoredTextEditor editor;

        protected override async Task OnInitializedAsync()
        {
            User = await HttpClient.GetFromJsonAsync<MUser>("api/users/me");
        }

        private bool isLoading = false;
        private async Task SubmitAsync()
        {
            isLoading = true;
            var user = MUser.FromUser(User);
            user.TermsAndConditions = await editor.GetHTML();
            if (user.TermsAndConditions == "<p><br></p>")
                user.TermsAndConditions = null;

            if (avatarPreview != null)
            {
                var response = await HttpClient.PostAsJsonAsync("api/images", avatarPreview);
                user.AvatarImageId = int.Parse(await response.Content.ReadAsStringAsync());
            }

            await HttpClient.PutAsJsonAsync("api/users", user);
            NavigationManager.NavigateTo(NavigationManager.Uri, true);
            isLoading = false;
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
