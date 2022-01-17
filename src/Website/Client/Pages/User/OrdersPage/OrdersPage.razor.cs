using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.User.OrdersPage
{
    [Authorize]
    public partial class OrdersPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public List<MOrder> Orders { get; set; }
        public List<MProductCustomer> Customers { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Orders = await HttpClient.GetFromJsonAsync<List<MOrder>>("api/orders");
            Customers = await HttpClient.GetFromJsonAsync<List<MProductCustomer>>("api/products/my");
        }
    }
}
