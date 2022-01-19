using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using Website.Client.Extensions;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.ProductPage.Components
{
    public partial class EditVersionModal
    {
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        public MVersion Model { get; set; }

        public string VersionName { get; set; }

        public string BranchName { get; set; }

        public async Task ShowAsync(MVersion model, string branchName)
        {
            Model = model;
            VersionName = model.Name;
            BranchName = branchName;
            StateHasChanged();
            await JsRuntime.ShowModalStaticAsync(nameof(EditVersionModal));
        }

        private bool isLoading = false;
        public async Task SubmitAsync()
        {
            isLoading = true;
            await HttpClient.PutAsJsonAsync("api/versions", Model);
            isLoading = false;
            await JsRuntime.HideModalAsync(nameof(EditVersionModal));
        }
    }
}