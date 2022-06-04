using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Shared.Constants;
using Website.Shared.Models.Database;
using Website.Shared.Models.Children;
using Website.Components.Helpers;

namespace Website.Client.Pages.Home.UserPage
{
    public partial class UserPage
    {
        [Parameter]
        public int UserId { get; set; }
        
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AuthenticatedUserService UserService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public UserProfile User { get; set; }

        private bool isLoaded = false;

        protected override async Task OnParametersSetAsync()
        {
            var response = await HttpClient.GetAsync($"api/users/{UserId}/profile");
            isLoaded = true;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                User = await response.Content.ReadFromJsonAsync<UserProfile>();
                User.Biography = MarkdownHelper.ParseToHtml(User.Biography);
            }
        }
        
        private void GoToProduct(MProduct product)
        {
            NavigationManager.NavigateTo($"/products/{product.Id}");
        }

        private enum EUserTab
        {
            Biography,
            Products
        }

        private EUserTab currentTab = EUserTab.Biography;

        private void ChangeTab(EUserTab tab)
        {
            currentTab = tab;
        }

        private string TabClass(EUserTab tab)
        {
            if (currentTab == tab)
                return "active";
            return string.Empty;
        }
    }
}