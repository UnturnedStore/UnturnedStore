using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Pages.Seller.CustomersPage.Components;
using Website.Components.Alerts;
using Website.Components.Basic;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.CustomersPage
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class CustomersPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public List<MProductCustomer> Customers { get; set; }

        private List<MProductCustomer> OrderedCustomers => Customers.OrderByDescending(x => x.CreateDate).ToList();

        public ConfirmModal<MProductCustomer> DeleteConfirmModal { get; set; }
        public AddCustomerModal AddModal { get; set; }
        public DetailsCustomerModal DetailsModal { get; set; }

        protected override async  Task OnInitializedAsync()
        {
            Customers = await HttpClient.GetFromJsonAsync<List<MProductCustomer>>("api/seller/customers");
        }

        private async Task ShowDeleteCustomerAsync(MProductCustomer customer)
        {
            await DeleteConfirmModal.ShowAsync(customer);
        }

        private async Task DeleteCustomerAsync(MProductCustomer customer)
        {
            await HttpClient.DeleteAsync("api/products/customers/" + customer.Id);
            Customers.Remove(customer);
        }

        public async Task ShowAddModalAsync()
        {
            await AddModal.ShowAsync();
        }

        public void AddCustomer(MProductCustomer customer)
        {
            Customers.Add(customer);
        }

        private async Task ShowDetailsModalAsync(MProductCustomer customer)
        {
            await DetailsModal.ShowAsync(customer.Id);
        }
    }
}