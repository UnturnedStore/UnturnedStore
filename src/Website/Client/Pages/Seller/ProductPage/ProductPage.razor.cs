using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Client.Pages.Seller.ProductPage
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class ProductPage
    {
        [Parameter]
        public int ProductId { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        public MProduct Product { get; set; }

        private HttpStatusCode statusCode { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var response = await HttpClient.GetAsync("api/seller/products/" + ProductId);
            statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.OK)
                Product = await response.Content.ReadFromJsonAsync<MProduct>();
        }
    }
}
