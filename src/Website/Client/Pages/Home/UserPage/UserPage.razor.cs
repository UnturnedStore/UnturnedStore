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

namespace Website.Client.Pages.Home.UserPage
{
    public partial class UserPage
    {
        [Parameter]
        public int UserId { get; set; }
        
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public UserService UserService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public MUserProfile User { get; set; }

        private bool isLoaded = false;

        protected override async Task OnParametersSetAsync()
        {
            var response = await HttpClient.GetAsync($"api/users/{UserId}/profile");
            isLoaded = true;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                User = await response.Content.ReadFromJsonAsync<MUserProfile>();
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