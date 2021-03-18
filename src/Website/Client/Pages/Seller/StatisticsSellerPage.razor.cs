using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.PieChart;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Client.Pages.Seller
{
    public partial class StatisticsSellerPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public IEnumerable<OrderModel> Orders { get; set; }
        private IEnumerable<OrderItemModel> OrderItems => Orders.SelectMany(x => x.Items);

        protected override async Task OnInitializedAsync()
        {
            Lables = new List<string>();
            Values = new List<double>();
            Orders = await HttpClient.GetFromJsonAsync<OrderModel[]>("api/seller/orders");
        }

        public List<string> Lables { get; set; }
        public List<double> Values { get; set; }


    }
}
