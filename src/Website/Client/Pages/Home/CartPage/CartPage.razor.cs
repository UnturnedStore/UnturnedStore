using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Website.Client.Pages.Home.CartPage.Components;
using Website.Client.Services;
using Website.Shared.Models.Database;
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

        public async Task ShowTermsModalAsync(MUser seller)
        {
            await TermsModal.ShowAsync(seller);
        }
    }
}
