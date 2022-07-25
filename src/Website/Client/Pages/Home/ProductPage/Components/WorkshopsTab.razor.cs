using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models.Database;
using Website.Shared.Results;

namespace Website.Client.Pages.Home.ProductPage.Components
{
    public partial class WorkshopsTab
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        [Parameter]
        public MProduct Product { get; set; }

        public WorkshopItemResult WorkshopResult { get; set; }

        public RequiredWorkshopsModal RequiredWorkshopsModal { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/products/workshop/verify", Product.WorkshopItems);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                WorkshopResult = await response.Content.ReadFromJsonAsync<WorkshopItemResult>();
            }
        }
    }
}
