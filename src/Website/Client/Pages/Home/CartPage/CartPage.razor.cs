using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Pages.Home.CartPage.Components;
using Website.Client.Services;
using Website.Shared.Constants;
using Website.Shared.Models;
using Website.Shared.Params;

namespace Website.Client.Pages.Home.CartPage
{
    public partial class CartPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public CartService CartService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public TermsModal TermsModal { get; set; }

        private List<OrderParams> Carts => CartService.Carts;

        protected override async Task OnInitializedAsync()
        {
            await CartService.ReloadCartAsync();
        }

        private async Task RemoveFromCartAsync(OrderParams cart, OrderItemParams item)
        {   
            await CartService.RemoveFromCartAsync(cart, item);
        }

        private bool isWaitingForProvider = false;
        private async Task CheckoutPayPalAsync(OrderParams orderParams)
        {
            if (!orderParams.IsAgree)
                return;

            orderParams.PaymentMethod = OrderConstants.Methods.PayPal;
            HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/orders", orderParams);

            MOrder order = await response.Content.ReadFromJsonAsync<MOrder>();
            isWaitingForProvider = true;
            NavigationManager.NavigateTo($"api/orders/{order.Id}/pay", true);
            await CartService.RemoveCartAsync(orderParams);
        }

        public async Task ShowTermsModalAsync(MUser seller)
        {
            await TermsModal.ShowAsync(seller);
        }
    }
}
