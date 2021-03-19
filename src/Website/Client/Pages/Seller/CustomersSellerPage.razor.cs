using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Pages.Seller.Components;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Client.Pages.Seller
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class CustomersSellerPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public List<ProductCustomerModel> Customers { get; set; }

        private List<ProductCustomerModel> OrderedCustomers => Customers.OrderByDescending(x => x.CreateDate).ToList();

        public AddCustomerModal Modal { get; set; }

        protected override async  Task OnInitializedAsync()
        {
            Customers = await HttpClient.GetFromJsonAsync<List<ProductCustomerModel>>("api/seller/customers");
        }

        private async Task DeleteCustomerAsync(ProductCustomerModel customer)
        {
            await HttpClient.DeleteAsync("api/products/customers/" + customer.Id);
            Customers.Remove(customer);
        }

        public async Task ShowModalAsync()
        {
            await Modal.ShowAsync();
        }

        public void AddCustomer(ProductCustomerModel customer)
        {
            Customers.Add(customer);
        }
    }
}
