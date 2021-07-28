using Blazored.TextEditor;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Client.Pages.User.SettingsPage.Components
{
    public partial class SellerTab
    {
        [Parameter]
        public MUser User { get; set; }

        private BlazoredTextEditor editor;

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public bool IsLoading { get; set; }
        [Parameter]
        public EventCallback<bool> IsLoadingChanged { get; set; }

        private async Task SubmitAsync()
        {
            IsLoading = true;
            var user = MUser.FromUser(User);

            user.TermsAndConditions = await editor.GetHTML();
            if (user.TermsAndConditions == "<p><br></p>")
                user.TermsAndConditions = null;

            await HttpClient.PutAsJsonAsync("api/users/seller", user);
            NavigationManager.NavigateTo(NavigationManager.Uri, true);
            IsLoading = false;
        }
    }
}
