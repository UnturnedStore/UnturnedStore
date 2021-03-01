using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Shared.Constants;
using Website.Shared.Models;
using Website.Shared.Params;

namespace Website.Client.Pages
{
    public partial class CartPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public CartService CartService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private List<OrderParams> Carts => CartService.Carts;

        protected override async Task OnInitializedAsync()
        {
            await CartService.ReloadCartAsync();
        }

        private async Task RemoveFromCartAsync(OrderParams orderParams, OrderItemParams item)
        {
            await CartService.RemoveFromCartAsync(orderParams, item);
        }

        private async Task CheckoutPayPalAsync(OrderParams orderParams)
        {
            orderParams.PaymentMethod = PaymentContants.PayPal;
            var response = await HttpClient.PostAsJsonAsync("api/orders", orderParams);

            var order = await response.Content.ReadFromJsonAsync<OrderModel>();
            NavigationManager.NavigateTo(order.PaymentUrl, true);
            await CartService.RemoveCartAsync(orderParams);
        }
    }
}
