using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.User.SettingsPage.Components
{
    public partial class SellerTab
    {
        [Parameter]
        public MUser User { get; set; }

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
            MUser user = MUser.FromUser(User);

            await HttpClient.PutAsJsonAsync("api/users/seller", user);
            NavigationManager.NavigateTo(NavigationManager.Uri, true);
            IsLoading = false;
        }
    }
}
