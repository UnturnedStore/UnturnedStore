using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Extensions;
using Website.Components.Alerts;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.OffersPage.Components
{
    public partial class DetailsProductSaleModal
    {
        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public HttpClient HttpClient { get; set; }

        [Inject] public AlertService AlertService { get; set; }

        public MProductSale Model { get; set; }

        public async Task ShowAsync(MProductSale sale)
        {
            Model = sale;
            StateHasChanged();

            await JsRuntime.ShowModalStaticAsync(nameof(DetailsProductSaleModal));
        }
    }
}