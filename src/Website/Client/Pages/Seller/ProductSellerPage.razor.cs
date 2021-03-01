using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
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
    public partial class ProductSellerPage
    {
        [Parameter]
        public int ProductId { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        public ProductModel Product { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            Product = await HttpClient.GetFromJsonAsync<ProductModel>("api/seller/products/" + ProductId);
        }
    }
}
