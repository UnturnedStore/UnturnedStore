using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Shared.Models.Database;
using Website.Shared.Results;

namespace Website.Client.Pages.Home.IndexPage
{
    public partial class Index
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public HomeStatisticsResult Statistics { get; set; }
        public IEnumerable<MProduct> Products { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Statistics = await HttpClient.GetFromJsonAsync<HomeStatisticsResult>("api/home/statistics");
            Products = await HttpClient.GetFromJsonAsync<MProduct[]>("api/home/promoted");
        }
    }
}
