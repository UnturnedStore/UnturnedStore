using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Models.Database;
using System.Net.Http.Json;

namespace Website.Client.Pages.Seller.UploadVersionPage
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class UploadVersionPage
    {
        [Parameter]
        public int ProductId { get; set; }

        public MProduct Product { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private MVersion defaultModel => new MVersion()
        {
            IsEnabled = true,
            BranchId = Product.Branches.FirstOrDefault()?.Id ?? 0
        };

        private string selectedTemplate = "rocketplugin";

        public MVersion Model { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            Product = await HttpClient.GetFromJsonAsync<MProduct>("api/seller/products/" + ProductId);
            Model = defaultModel;
        }

        private void DefaultSelectedTemplate()
        {
            selectedTemplate = "default";
        }

        private string msg = null;

        private bool isLoading;
        private async Task SubmitAsync()
        {
            if (Model.Content == null)
            {
                msg = "The file field is required";
                return;
            }
            if (!Model.FileName.EndsWith(".zip"))
            {
                msg = "The file must be a file of type: zip";
                return;
            }

            isLoading = true;
            var response = await HttpClient.PostAsJsonAsync("api/versions", Model);

            var version = await response.Content.ReadFromJsonAsync<MVersion>();
            Product.Branches.First(x => x.Id == version.BranchId).Versions.Add(version);
            Model = defaultModel;
            isLoading = false;
            StateHasChanged();
            NavigationManager.NavigateTo($"/seller/products/{ProductId}");
        }
    }
}
