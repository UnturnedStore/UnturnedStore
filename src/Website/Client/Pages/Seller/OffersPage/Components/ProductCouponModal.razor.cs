using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Extensions;
using Website.Components.Alerts;
using Website.Shared.Constants;
using Website.Shared.Enums;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.OffersPage.Components
{
    public partial class ProductCouponModal
    {
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }

        [Parameter]
        public List<MProduct> Products { get; set; }
        public IEnumerable<MProduct> SearchedProducts => Products.Where(x => x.IsEnabled && x.Price > 0 && x.Status == ProductStatus.Released && x.Name.Contains(searchProduct, StringComparison.OrdinalIgnoreCase)).Take(3);

        [Parameter]
        public EventCallback<MProductCoupon> OnCouponAdded { get; set; }
        [Parameter]
        public EventCallback<MProductCoupon> OnCouponEdited { get; set; }

        public MProductCoupon Model { get; set; }
        
        public void GenerateNewCouponCode()
        {
            Model.CouponCode = Guid.NewGuid().ToString("N").Substring(0, 16);
        }

        private string searchProduct = string.Empty;

        private void ResetSearchProduct()
        {
            searchProduct = string.Empty;
        }

        private void ChangeProduct(MProduct product)
        {
            Model.Product = product;
            Model.ProductId = product.Id;
            ResetSearchProduct();
        }

        private void ResetProduct()
        {
            Model.Product = null;
            Model.ProductId = 0;
        }

        public string MaxUses
        {
            get => Model.MaxUses > 0 ? Model.MaxUses.ToString() : string.Empty;
            set
            {
                if (string.IsNullOrEmpty(value)) Model.MaxUses = 0;
                else if (int.TryParse(value, out int result) && result >= 0) 
                { 
                    Model.MaxUses = result;
                }
            }
        }

        private bool isEditing { get; set; }

        public async Task ShowAddAsync()
        {
            Model = new MProductCoupon() { CouponMultiplier = 1M, MaxUses = 0, IsEnabled = true, CouponUsageCount = 0 };
            isEditing = false;
            StateHasChanged();
            await JsRuntime.ShowModalStaticAsync(nameof(ProductCouponModal));
        }

        public async Task ShowEditAsync(MProductCoupon model)
        {
            Model = new MProductCoupon(model);
            isEditing = true;
            StateHasChanged();
            await JsRuntime.ShowModalStaticAsync(nameof(ProductCouponModal));
        }

        private bool isLoading = false;
        public async Task SubmitAsync()
        {
            isLoading = true;

            if (!isEditing)
            {
                MProduct product = Model.Product;
                Model.Product = null;
                HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/offers/coupons", Model);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        AlertService.ShowAlert("product-coupons-main", $"There's already a coupon with this product and code combination", AlertType.Danger);
                    }
                    else
                    {
                        AlertService.ShowAlert("product-coupons-main", $"An error occurred <strong>{response.StatusCode}</strong>", AlertType.Danger);
                    }
                }
                else
                {
                    Model = await response.Content.ReadFromJsonAsync<MProductCoupon>();
                    Model.Product = product;
                    Model.CouponUsageCount = 0;
                    await OnCouponAdded.InvokeAsync(Model);

                    AlertService.ShowAlert("product-coupons-main", $"Successfully created new coupon <strong>{Model.CouponName}</strong>!", AlertType.Success);

                    await JsRuntime.HideModalAsync(nameof(ProductCouponModal));
                    Model = new MProductCoupon();
                    StateHasChanged();
                }
            }
            else
            {
                HttpResponseMessage response = await HttpClient.PutAsJsonAsync("api/offers/coupons", Model);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        AlertService.ShowAlert("product-coupons-main", $"There's already a coupon with this product and code combination", AlertType.Danger);
                    }
                    else
                    {
                        AlertService.ShowAlert("product-coupons-main", $"An error occurred <strong>{response.StatusCode}</strong>", AlertType.Danger);
                    }
                }
                else
                {
                    await OnCouponEdited.InvokeAsync(Model);

                    AlertService.ShowAlert("product-coupons-main", $"Successfully edited coupon <strong>{Model.CouponName}</strong>!", AlertType.Success);

                    await JsRuntime.HideModalAsync(nameof(ProductCouponModal));
                    Model = new MProductCoupon();
                    StateHasChanged();
                }
            }

            isLoading = false;

            await JsRuntime.HideModalAsync(nameof(ProductCouponModal));
        }
    }
}