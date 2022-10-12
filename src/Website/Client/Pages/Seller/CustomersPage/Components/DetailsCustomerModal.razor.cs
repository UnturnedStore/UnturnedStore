using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Extensions;
using Website.Components.Alerts;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.CustomersPage.Components
{
    public partial class DetailsCustomerModal
    {
        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public HttpClient HttpClient { get; set; }

        [Inject] public AlertService AlertService { get; set; }

        [Parameter] public List<MProductCustomer> Customers { get; set; }
        [Parameter] public EventCallback<MProductCustomer> OnStateChanged { get; set; }

        public MProductCustomer Model { get; set; }

        private bool isSuspended;
        public async Task ShowAsync(int customerId)
        {
            message = null;
            Model = Customers.FirstOrDefault(c => c.Id == customerId);
            isSuspended = Model.IsBlocked;

            await JsRuntime.ShowModalStaticAsync(nameof(DetailsCustomerModal));
        }

        private string message = null;
        private bool isLoading = false;
        public async Task SubmitAsync()
        {
            isLoading = true;

            var tempModel = new MProductCustomer();
            tempModel.Id = Model.Id;
            tempModel.ProductId = Model.ProductId;
            tempModel.UserId = Model.UserId;
            tempModel.IsBlocked = isSuspended;
            tempModel.BlockDate = isSuspended ? DateTime.UtcNow : null;
            var response = await HttpClient.PutAsJsonAsync("api/products/customers", tempModel);

            isLoading = false;

            if (!response.IsSuccessStatusCode)
            {
                message = $"An error occurated <strong>{response.StatusCode}</strong>";
            }
            else
            {
                Model.IsBlocked = tempModel.IsBlocked;
                Model.BlockDate = tempModel.BlockDate;

                if (Model.IsBlocked)
                {
                    AlertService.ShowAlert("customers-main", $"Product Customer <strong>{Model.Id}</strong> successfully Suspended", AlertType.Success);
                }
                else
                {
                    AlertService.ShowAlert("customers-main", $"Product Customer <strong>{Model.Id}</strong> successfully Restored", AlertType.Success);
                }

                await OnStateChanged.InvokeAsync(Model);

                await JsRuntime.HideModalAsync(nameof(DetailsCustomerModal));
            }
        }


        public void SuspendCustomer()
        {
            isSuspended = true;
        }
        public void RestoreCustomer()
        {
            isSuspended = false;
        }
    }
}