using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Components.Alerts;
using Website.Components.Basic;
using Website.Shared.Constants;
using Website.Shared.Enums;
using Website.Shared.Models;
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
        [Inject]
        public AlertService AlertService { get; set; }
        [Inject]
        public AuthenticatedUserService UserService { get; set; }

        public SellerProduct Product { get; set; }

        private HttpStatusCode statusCode { get; set; }

        public string StatusReason { get; set; }

        public ConfirmModal<ProductInfo> ConfirmRelease { get; set; }
        public ConfirmModal<ProductInfo> ConfirmDisable { get; set; }
        public ConfirmModal<ProductInfo> ConfirmSubmit { get; set; }
        public ConfirmModal<ProductInfo> ConfirmReject { get; set; }
        public ConfirmModal<ProductInfo> ConfirmApprove { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var response = await HttpClient.GetAsync("api/seller/products/" + ProductId);
            statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.OK)
                Product = await response.Content.ReadFromJsonAsync<SellerProduct>();
        }

        private async Task HandleRelease()
        {
            await ConfirmRelease.ShowAsync(Product);
        }

        private async Task HandleDisable()
        {
            await ConfirmDisable.ShowAsync(Product);
        }

        private async Task HandleSubmit()
        {
            await ConfirmSubmit.ShowAsync(Product);
        }

        private async Task HandleReject()
        {
            await ConfirmReject.ShowAsync(Product);
        }

        private async Task HandleApprove()
        {
            await ConfirmApprove.ShowAsync(Product);
        }

        private async Task SubmitReleaseAsync()
        {
            bool isSuccess = await ChangeStatusAsync(ProductStatus.Released);
            if (isSuccess)
            {
                Product.IsEnabled = true;
                AlertService.ShowAlert("product-main", $"Successfully released <strong>{Product.Name}</strong>!", AlertType.Success);
            }

            StateHasChanged();
        }

        private async Task SubmitDisableAsync()
        {
            bool isSuccess = await ChangeStatusAsync(ProductStatus.Disabled, StatusReason);
            if (isSuccess)
            {
                StatusReason = null;
                Product.IsEnabled = false;
                AlertService.ShowAlert("product-main", $"Successfully disabled <strong>{Product.Name}</strong>!", AlertType.Success);
            }

            StateHasChanged();
        }

        private async Task SubmitRequestAsync()
        {
            bool isSuccess = await ChangeStatusAsync(ProductStatus.WaitingForApproval);
            if (isSuccess)
                AlertService.ShowAlert("product-main", $"Successfully send approval request for <strong>{Product.Name}</strong>!", AlertType.Success);

            StateHasChanged();
        }

        private async Task SubmitRejectAsync()
        {
            bool isSuccess = await ChangeStatusAsync(ProductStatus.Rejected, StatusReason);
            if (isSuccess)
            {
                StatusReason = null;
                AlertService.ShowAlert("product-main", $"Successfully rejected <strong>{Product.Name}</strong>!", AlertType.Success);
            }

            StateHasChanged();
        }

        private async Task SubmitApproveAsync()
        {
            bool isSuccess = await ChangeStatusAsync(ProductStatus.Approved);
            if (isSuccess)
                AlertService.ShowAlert("product-main", $"Successfully approved <strong>{Product.Name}</strong>!", AlertType.Success);

            StateHasChanged();
        }

        private async Task<bool> ChangeStatusAsync(ProductStatus status, string reason = null)
        {
            ChangeProductStatusParams @params = new ChangeProductStatusParams()
            {
                ProductId = ProductId,
                Status = status,
                StatusReason = reason
            };

            HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/products/status", @params);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Product.Status = @params.Status;
                return true;
            }
            else
            {
                AlertService.ShowAlert("product-main", $"An error occurred {response.StatusCode}", AlertType.Danger);
            }
            return false;
        }
    }
}
