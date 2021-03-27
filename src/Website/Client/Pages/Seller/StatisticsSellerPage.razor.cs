using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Client.Pages.Seller
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class StatisticsSellerPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public IEnumerable<OrderModel> Orders { get; set; }
        private OrderItemModel[] OrderItems { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Lables = new List<string>();
            Values = new List<double>();
            Orders = await HttpClient.GetFromJsonAsync<OrderModel[]>("api/seller/orders");
            OrderItems = Orders.SelectMany(x => x.Items).ToArray();
            foreach (var item in OrderItems)
                item.Order = Orders.First(x => x.Id == item.OrderId);
        }

        public List<string> Lables { get; set; }
        public List<double> Values { get; set; }
    }
}
