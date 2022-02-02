using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Components.Basic;
using Website.Shared.Constants;
using Website.Shared.Enums;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Client.Pages.Seller.ProductPage
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class ProductPage
    {
        [Parameter]
        public int ProductId { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        public MProduct Product { get; set; }

        private HttpStatusCode statusCode { get; set; }

        public ConfirmModal<MProduct> ConfirmRelease { get; set; }
        public ConfirmModal<MProduct> ConfirmSubmit { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var response = await HttpClient.GetAsync("api/seller/products/" + ProductId);
            statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.OK)
                Product = await response.Content.ReadFromJsonAsync<MProduct>();
        }

        private async Task HandleRelease()
        {
            await ConfirmRelease.ShowAsync(Product);
        }

        private async Task HandleSubmit()
        {
            await ConfirmSubmit.ShowAsync(Product);
        }

        private async Task SubmitReleaseAsync(MProduct product)
        {
            await ChangeStatusAsync(ProductStatus.Released);
            StateHasChanged();
        }

        private async Task SubmitRequestAsync(MProduct product)
        {
            await ChangeStatusAsync(ProductStatus.WaitingForApproval);
            StateHasChanged();
        }

        private async Task ChangeStatusAsync(ProductStatus status)
        {
            ChangeProductStatusParams @params = new ChangeProductStatusParams()
            {
                ProductId = ProductId,
                Status = status
            };

            HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/products/status", @params);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Product.Status = @params.Status;
            }
        }
    }
}
