using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using Website.Client.Extensions;
using Website.Shared.Models.Database;
using Website.Components.Alerts;
using System.Net;
using Website.Shared.Results;
using System.Collections.Generic;
using System;
using System.Linq;
using Website.Shared.Enums;
using Website.Shared.Constants;

namespace Website.Client.Pages.Seller.OffersPage.Components
{
    public partial class ProductSaleModal
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
        public List<MProductSale> ProductSales { get; set; }

        [Parameter]
        public EventCallback<MProductSale> OnSaleAdded { get; set; }
        [Parameter]
        public EventCallback<MProductSale> OnSaleEdited { get; set; }

        public MProductSale Model { get; set; }

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
            AllowedStartDate = GetAllowedStartDate(Model.ProductId);
        }

        private void ResetProduct()
        {
            Model.Product = null;
            Model.ProductId = 0;
            AllowedStartDate = DateTime.Now;
            message = null;
        }

        public DateTime AllowedStartDate { get; set; }

        public DateTime GetAllowedStartDate(int productId)
        {
            if (ProductSales == null) return DateTime.Now;
            return ProductSales.Where(s => s.ProductId == productId && s.Id != Model.Id).OrderByDescending(s => s.StartDate).FirstOrDefault()?.EndDate.AddDays(ProductSalesConstants.SaleCooldownDays) ?? DateTime.Now;
        }

        public DateTime StartDate
        {
            get => Model.SaleStart;
            set
            {
                if (value > DateTime.Now)
                {
                    Model.StartDate = value;
                    if (EndDate < value.AddDays(1)) EndDate = value.AddDays(1);
                }
                else
                {
                    Model.StartDate = null;
                }
            }
        }

        public DateTime EndDate
        {
            get => Model.EndDate;
            set
            {
                if (value > StartDate)
                {
                    Model.EndDate = value;
                }
                else
                {
                    Model.EndDate = StartDate.AddDays(1);
                }
            }
        }

        private bool isEditing { get; set; }

        public async Task ShowAddAsync()
        {
            Model = new MProductSale() { SaleMultiplier = 1M, EndDate = DateTime.Now.AddDays(31), SaleUsageCount = 0 };
            AllowedStartDate = DateTime.Now;
            message = null;
            isEditing = false;
            StateHasChanged();
            await JsRuntime.ShowModalStaticAsync(nameof(ProductSaleModal));
        }

        public async Task ShowEditAsync(MProductSale model)
        {
            Model = new MProductSale(model);
            AllowedStartDate = GetAllowedStartDate(Model.ProductId);
            message = null;
            isEditing = true;
            StateHasChanged();
            await JsRuntime.ShowModalStaticAsync(nameof(ProductSaleModal));
        }

        private string message { get; set; }

        private bool isLoading = false;
        public async Task SubmitAsync()
        {
            if (Model.SaleStart < AllowedStartDate)
            {
                message = $"Start date must be after <strong>{AllowedStartDate.ToString("d")}</strong>, at least 2 months after this product's last sale";
                return;
            }
            else if ((Model.EndDate - Model.SaleStart) > TimeSpan.FromDays(ProductSalesConstants.SaleMaxDurationDays))
            {
                message = $"Total duration cannot exceed 1 month";
                return;
            }
            else message = null;

            isLoading = true;

            if (!isEditing)
            {
                bool defaultStartDate = !Model.StartDate.HasValue;
                MProduct product = Model.Product;
                HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/offers/sales", Model);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    AlertService.ShowAlert("product-sales-main", $"An error occurred <strong>{response.StatusCode}</strong>", AlertType.Danger);
                }
                else
                {
                    Model = await response.Content.ReadFromJsonAsync<MProductSale>();
                    Model.Product = product;
                    if (defaultStartDate) Model.IsActive = true;
                    Model.SaleUsageCount = 0;
                    await OnSaleAdded.InvokeAsync(Model);

                    AlertService.ShowAlert("product-sales-main", $"Successfully created new sale <strong>{Model.SaleName}</strong>!", AlertType.Success);

                    await JsRuntime.HideModalAsync(nameof(ProductSaleModal));
                    Model = new MProductSale();
                    StateHasChanged();
                }
            }
            else
            {
                HttpResponseMessage response = await HttpClient.PutAsJsonAsync("api/offers/sales", Model);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    AlertService.ShowAlert("product-sales-main", $"An error occurred <strong>{response.StatusCode}</strong>", AlertType.Danger);
                }
                else
                {
                    if (!Model.StartDate.HasValue) Model.IsActive = true;
                    await OnSaleEdited.InvokeAsync(Model);

                    AlertService.ShowAlert("product-sales-main", $"Successfully edited sale <strong>{Model.SaleName}</strong>!", AlertType.Success);

                    await JsRuntime.HideModalAsync(nameof(ProductSaleModal));
                    Model = new MProductSale();
                    StateHasChanged();
                }
            }

            isLoading = false;

            await JsRuntime.HideModalAsync(nameof(ProductSaleModal));
        }
    }
}