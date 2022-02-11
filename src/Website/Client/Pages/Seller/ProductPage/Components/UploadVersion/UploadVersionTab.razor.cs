using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Pages.Seller.ProductPage.Components.UploadVersion.Modals;
using Website.Components.Alerts;
using Website.Shared.Models;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.ProductPage.Components.UploadVersion
{
    public partial class UploadVersionTab
    {
        [Parameter]
        public SellerProduct Product { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }

        public MVersion Version { get; set; }

        private MVersion defaultVersion => new MVersion()
        {
            IsEnabled = true,
            BranchId = Product.Branches.FirstOrDefault()?.Id ?? 0
        };

        private MBranch currentBranch => Product.Branches.First(x => x.Id == Version.BranchId);
        private MVersion latestVersion => currentBranch.Versions.OrderByDescending(x => x.CreateDate).FirstOrDefault();

        protected override void OnInitialized()
        {
            Version = defaultVersion;
        }

        private PluginUploadModal PluginModal { get; set; }

        private bool IsDisabled => Version.Content == null || string.IsNullOrEmpty(Version.Changelog) || string.IsNullOrEmpty(Version.Name);

        private bool isLoading;
        private async Task SubmitAsync()
        {
            isLoading = true;
            var response = await HttpClient.PostAsJsonAsync("api/versions", Version);

            var version = await response.Content.ReadFromJsonAsync<MVersion>();

            currentBranch.Versions.Add(version);

            AlertService.ShowAlert("uploadversiontab-main",
                $"Successfully uploaded new version <strong>{currentBranch.Name}</strong> <strong>{Version.Name}</strong>!", AlertType.Success);

            Version = defaultVersion;
            isLoading = false;            

            StateHasChanged();
        }
    }
}
