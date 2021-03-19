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

        private bool isLoading;
        private async Task SubmitAsync()
        {
            isLoading = true;
            var response = await HttpClient.PostAsJsonAsync("api/versions", Model);

            var plugin = await response.Content.ReadFromJsonAsync<VersionModel>();
            Product.Branches.First(x => x.Id == plugin.BranchId).Versions.Add(plugin);
            Model = defaultModel;
            isLoading = false;
            StateHasChanged();
            NavigationManager.NavigateTo($"/seller/products/{ProductId}");
        }
    }
}
