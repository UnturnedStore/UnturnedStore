using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Website.Shared.Models.Database;
using Website.Shared.Models;
using Website.Components.Alerts;
using System.Net;

namespace Website.Client.Pages.Seller.ProductPage.Components
{
    public partial class BranchesTab
    {
        [Parameter]
        public SellerProduct Product { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }

        public EditVersionModal EditVersionModal { get; set; }

        public int SelectedBranchId { get; set; }

        private MBranch createBranch;
        private MBranch Branch
        {
            get
            {
                if (SelectedBranchId != 0)
                {
                    return Product.Branches.FirstOrDefault(x => x.Id == SelectedBranchId);
                }
                else
                {
                    return createBranch;
                }
            }
        }

        private List<MVersion> Versions => Branch.Versions.OrderByDescending(x => x.CreateDate).ToList();
        protected override void OnParametersSet()
        {
            SelectedBranchId = Product.Branches.FirstOrDefault()?.Id ?? 0;
        }

        public MVersion Plugin { get; set; }

        private async Task TogglePluginAsync(MVersion plugin)
        {
            await HttpClient.PatchAsync($"api/versions/{plugin.Id}", null);
            plugin.IsEnabled = !plugin.IsEnabled;
        }

        private void AddBranch()
        {
            createBranch = new MBranch()
            {ProductId = Product.Id};
            SelectedBranchId = 0;
        }

        private bool isLoading;
        private async Task SubmitAsync()
        {
            isLoading = true;
            if (Branch.Id == default)
            {
                var response = await HttpClient.PostAsJsonAsync("api/branches", Branch);
                
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    MBranch branch = await response.Content.ReadFromJsonAsync<MBranch>();
                    Product.Branches.Add(branch);
                    SelectedBranchId = branch.Id;

                    AlertService.ShowAlert("product-branches", $"Successfully added new branch <strong>{branch.Name}</strong>!", AlertType.Success);

                } else
                {
                    AlertService.ShowAlert("product-basicinfo", "There was an error when trying to add new branch!", AlertType.Danger);
                }
            }
            else
            {
                var response = await HttpClient.PutAsJsonAsync("api/branches", MBranch.FromBranch(Branch));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    AlertService.ShowAlert("product-branches", $"Successfully updated branch <strong>{Branch.Name}</strong>!", AlertType.Success);
                } else
                {
                    AlertService.ShowAlert("product-basicinfo", $"There was an error when trying to update <strong>{Branch.Name}</strong>!", AlertType.Danger);
                }
            }

            isLoading = false;
        }

        public async Task EditVersionAsync(MVersion version)
        {
            await EditVersionModal.ShowAsync(version, Branch.Name);
        }
    }
}