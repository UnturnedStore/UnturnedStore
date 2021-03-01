using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Client.Pages.User
{
    [Authorize]
    public partial class OrdersUserPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public List<OrderModel> Orders { get; set; }
        public List<ProductCustomerModel> Customers { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Orders = await HttpClient.GetFromJsonAsync<List<OrderModel>>("api/orders");
            Customers = await HttpClient.GetFromJsonAsync<List<ProductCustomerModel>>("api/products/my");
        }
    }
}
