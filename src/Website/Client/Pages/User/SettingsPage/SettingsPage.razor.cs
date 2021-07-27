using Blazored.TextEditor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
            
            await HttpClient.PutAsJsonAsync("api/users", user);
            NavigationManager.NavigateTo(NavigationManager.Uri, true);
            isLoading = false;
        }
    }
}
