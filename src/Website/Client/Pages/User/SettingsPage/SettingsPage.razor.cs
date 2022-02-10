using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models.Database;

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

        private bool isLoading = false;

        protected override async Task OnInitializedAsync()
        {
            User = await HttpClient.GetFromJsonAsync<MUser>("api/users/settings");
        }

        private enum ESettingsTab
        {
            Profile,
            Seller,
            Notifications
        }

        private ESettingsTab currentTab = ESettingsTab.Profile;

        private void ChangeTab(ESettingsTab tab)
        {
            currentTab = tab;
        }

        private string TabClass(ESettingsTab tab)
        {
            if (currentTab == tab)
                return "active";
            return string.Empty;
        }

        private string LoadingClass()
        {
            if (isLoading)
                return "disabled";
            else
                return string.Empty;
        }

    }
}
