using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Models;
using System.Net.Http.Json;

namespace Website.Client.Pages.Seller
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class UploadVersionSellerPage
    {
        [Parameter]
        public int ProductId { get; set; }

        public ProductModel Product { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private VersionModel defaultModel => new VersionModel()
        {
            IsEnabled = true,
            BranchId = Product.Branches.FirstOrDefault()?.Id ?? 0
        };

        private string selectedTemplate = "default";

        public VersionModel Model { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            Product = await HttpClient.GetFromJsonAsync<ProductModel>("api/seller/products/" + ProductId);
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

            var version = await response.Content.ReadFromJsonAsync<VersionModel>();
            Product.Branches.First(x => x.Id == version.BranchId).Versions.Add(version);
            Model = defaultModel;
            isLoading = false;
            StateHasChanged();
            NavigationManager.NavigateTo($"/seller/products/{ProductId}");
        }
    }
}
