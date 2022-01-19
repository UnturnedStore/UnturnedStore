using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.StatisticsPage
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class StatisticsPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public IEnumerable<MOrder> Orders { get; set; }
        private IEnumerable<MOrderItem> OrderItems { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Lables = new List<string>();
            Values = new List<double>();
            Orders = await HttpClient.GetFromJsonAsync<MOrder[]>("api/seller/orders");
            
            OrderItems = Orders.SelectMany(x => x.Items);

            foreach (var item in OrderItems)
                item.Order = Orders.First(x => x.Id == item.OrderId);

            OrderItems = OrderItems.OrderByDescending(x => x.Order.CreateDate);
        }

        public List<string> Lables { get; set; }
        public List<double> Values { get; set; }
    }
}
