using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.ProductPage.Components
{
    public partial class BranchesTab
    {
        [Parameter]
        public MProduct Product { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

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
                var branch = await response.Content.ReadFromJsonAsync<MBranch>();
                Product.Branches.Add(branch);
                SelectedBranchId = branch.Id;
            }
            else
            {
                await HttpClient.PutAsJsonAsync("api/branches", MBranch.FromBranch(Branch));
            }

            isLoading = false;
        }

        public async Task EditVersionAsync(MVersion version)
        {
            await EditVersionModal.ShowAsync(version, Branch.Name);
        }
    }
}