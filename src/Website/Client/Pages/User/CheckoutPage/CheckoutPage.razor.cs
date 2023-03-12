using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Components.Alerts;
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
        public AlertService AlertService { get; set; }
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
                OrderParams.Seller.TermsAndConditions = MarkdownHelper.ParseToHtml(OrderParams.Seller.TermsAndConditions, false)
                                                        .Replace("<PluginName>", string.Join(", ", OrderParams.Items.Select(o => o.Product.Name)));
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

        private string CouponCode { get; set; }

        private async Task HandleCouponSubmit(KeyboardEventArgs e)
        {
            if (e.Code != "Enter" && e.Code != "NumpadEnter") return;
            await GetCoupon(CouponCode);
        }

        private async Task GetCoupon(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                AlertService.HideAlert("user-checkout-coupon");
                return;
            }
            else if (couponCode.Length > 16)
            {
                AlertService.ShowAlert("user-checkout-coupon", "Invalid coupon code", AlertType.Danger);
                return;
            }

            HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/offers/coupons/" + couponCode, OrderParams.Items);
            if (response.IsSuccessStatusCode)
            {
                MProductCoupon coupon = await response.Content.ReadFromJsonAsync<MProductCoupon>();
                for (int i = 0; i < OrderParams.Items.Count; i++)
                    if (OrderParams.Items[i].ProductId == coupon.ProductId)
                    {
                        OrderParams.Items[i].CouponCode = coupon.CouponCode;
                        OrderParams.Items[i].Coupon = coupon;
                        coupon.Product = OrderParams.Items[i].Product;
                        break;
                    }

                AlertService.ShowAlert("user-checkout-coupon", $"Successfully found and applied coupon {coupon.CouponName} to {coupon.Product.Name}", AlertType.Success);
                CouponCode = string.Empty;
            } else
            {
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    AlertService.ShowAlert("user-checkout-coupon", "Found a coupon but it doesn't affect any of the products in your cart", AlertType.Primary);
                } else
                {
                    AlertService.ShowAlert("user-checkout-coupon", "Invalid coupon code", AlertType.Danger);
                }
            }
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
