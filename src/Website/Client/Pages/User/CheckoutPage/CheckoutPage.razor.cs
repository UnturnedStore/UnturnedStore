using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Components.Helpers;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Client.Pages.User.CheckoutPage
{
    [Authorize]
    public partial class CheckoutPage
    {
        [Parameter]
        public int SellerId { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public CartService CartService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public string[] PaymentMethods { get; set; } = new string[0];
        public OrderParams OrderParams { get; set; }

        private bool isLoaded = false;
        protected override async Task OnParametersSetAsync()
        {
            OrderParams = CartService.GetOrderParams(SellerId);
            
            if (OrderParams != null)
            {
                OrderParams.Seller.TermsAndConditions = MarkdownHelper.ParseToHtml(OrderParams.Seller.TermsAndConditions, false);
                PaymentMethods = await HttpClient.GetFromJsonAsync<string[]>($"api/payments/{SellerId}");
                if (string.IsNullOrEmpty(OrderParams.PaymentMethod))
                {
                    await ChangePaymentMethod(PaymentMethods.FirstOrDefault());
                }
            }

            isLoaded = true;
        }

        private async Task ChangePaymentMethod(string paymentMethod)
        {
            OrderParams.PaymentMethod = paymentMethod;
            await CartService.UpdateCartAsync();
        }

        private bool IsChecked(string paymentProvider)
        {
            if (OrderParams.PaymentMethod == paymentProvider)
                return true;
            return false;
        }

        private string BtnDisabled => !OrderParams.IsAgree ? "disabled" : string.Empty;
        private string BtnDisabledTitle => !OrderParams.IsAgree ? "You have to agree to terms & conditions" : null;

        private MOrder order;
        private bool IsDisabled => order != null;
        private async Task CreateOrderAsync()
        {
            if (!OrderParams.IsAgree)
                return;

            HttpResponseMessage msg = await HttpClient.PostAsJsonAsync("api/orders", OrderParams);
            order = await msg.Content.ReadFromJsonAsync<MOrder>();
            await CartService.RemoveCartAsync(OrderParams);
            NavigationManager.NavigateTo($"api/orders/{order.Id}/pay", true);
        }
    }
}
