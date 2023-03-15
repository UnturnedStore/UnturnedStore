using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Schema;
using Website.Client.Pages.Admin.ProductsPage.Components;
using Website.Client.Pages.Seller.OffersPage.Components;
using Website.Client.Services;
using Website.Components.Alerts;
using Website.Components.Basic;
using Website.Shared.Constants;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Client.Pages.Seller.OffersPage
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public partial class OffersPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }
        [Inject]
        public AuthenticatedUserService UserService { get; set; }

        public List<MProduct> Products { get; set; }
        public List<MProductSale> ProductSales { get; set; }
        public List<MProductCoupon> ProductCoupons { get; set; }

        private List<int> showCouponCodes = new List<int>();

        protected override async Task OnInitializedAsync()
        {
            Products = await HttpClient.GetFromJsonAsync<List<MProduct>>("api/seller/products");
            var sales = await HttpClient.GetFromJsonAsync<List<MProductSale>>("api/offers/sales");
            var coupons = await HttpClient.GetFromJsonAsync<List<MProductCoupon>>("api/offers/coupons");
            sales.ForEach(s => s.Product = Products.FirstOrDefault(p => p.Id == s.ProductId));
            coupons.ForEach(co => co.Product = Products.FirstOrDefault(p => p.Id == co.ProductId));
            ProductSales = sales;
            ProductCoupons = coupons;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (ProductSales == null) return;
            foreach (MProductSale sale in ProductSales)
            {
                if (!sale.IsActive && !sale.IsExpired && sale.StartDate < DateTime.Now)
                {
                    sale.IsActive = true;
                    sale.SaleUsageCount = 0;
                    sale.ProductPrice = sale.Product.Price;
                }
                else if (!sale.IsExpired && sale.EndDate < DateTime.Now)
                {
                    sale.IsActive = false;
                    sale.IsExpired = true;
                    sale.ProductPrice = sale.Product.Price;
                }
            }
        }

        public ProductSaleModal SaleModal { get; set; }
        public async Task ShowModalAddSaleAsync()
        {
            await SaleModal.ShowAddAsync();
        }

        public async Task ShowModalEditSaleAsync(MProductSale Sale)
        {
            await SaleModal.ShowEditAsync(Sale);
        }

        public void AddSale(MProductSale Sale)
        {
            ProductSales.Add(Sale);
        }

        public void EditSale(MProductSale newSale)
        {
            for (int t = 0; t < ProductSales.Count; t++)
                if (ProductSales[t].Id == newSale.Id)
                {
                    ProductSales[t] = newSale;
                    break;
                }
        }

        public DetailsProductSaleModal DetailsSaleModal { get; set; }
        public async Task ShowDetailsModalSaleAsync(MProductSale Sale)
        {
            await DetailsSaleModal.ShowAsync(Sale);
        }

        public ConfirmModal<MProductSale> ConfirmDeleteSale { get; set; }
        public async Task ShowDeleteSaleAsync(MProductSale Sale)
        {
            await ConfirmDeleteSale.ShowAsync(Sale);
        }

        public async Task DeleteSaleAsync(MProductSale Sale)
        {
            HttpResponseMessage response = await HttpClient.DeleteAsync("api/offers/sales/" + Sale.Id);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                ProductSales.Remove(Sale);
                AlertService.ShowAlert("product-sales-main", $"Successfully removed sale <span class=\"fw-bold\">{Sale.SaleName}</span>!", AlertType.Success);
            }
            else
            {
                AlertService.ShowAlert("product-sales-main", $"There was an error when trying to remove <span class=\"fw-bold\">{Sale.SaleName}</span>", AlertType.Danger);
            }
        }

        public ConfirmModal<MProductSale> ConfirmExpireSale { get; set; }
        public async Task ShowExpireSaleAsync(MProductSale Sale)
        {
            await ConfirmExpireSale.ShowAsync(Sale);
        }

        public async Task ExpireSaleAsync(MProductSale Sale)
        {
            HttpResponseMessage response = await HttpClient.DeleteAsync("api/offers/sales/" + Sale.Id + "/expire");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Sale.IsActive = false;
                Sale.IsExpired = true;
                Sale.ProductPrice = Sale.Product.Price;
                Sale.EndDate = DateTime.Now;
                AlertService.ShowAlert("product-sales-main", $"Successfully expired sale <span class=\"fw-bold\">{Sale.SaleName}</span>!", AlertType.Success);
            }
            else
            {
                AlertService.ShowAlert("product-sales-main", $"There was an error when trying to expire <span class=\"fw-bold\">{Sale.SaleName}</span>", AlertType.Danger);
            }
        }

        private void ShowCouponCode(int couponId)
        {
            showCouponCodes.Add(couponId);
        }

        public ProductCouponModal CouponModal { get; set; }
        public async Task ShowModalAddCouponAsync()
        {
            await CouponModal.ShowAddAsync();
        }

        public async Task ShowModalEditCouponAsync(MProductCoupon Coupon)
        {
            await CouponModal.ShowEditAsync(Coupon);
        }

        public void AddCoupon(MProductCoupon Coupon)
        {
            ProductCoupons.Add(Coupon);
        }

        public void EditCoupon(MProductCoupon newCoupon)
        {
            for (int t = 0; t < ProductCoupons.Count; t++)
                if (ProductCoupons[t].Id == newCoupon.Id)
                {
                    ProductCoupons[t] = newCoupon;
                    break;
                }
        }

        public ConfirmModal<MProductCoupon> ConfirmDeleteCoupon { get; set; }
        public async Task ShowDeleteCouponAsync(MProductCoupon Coupon)
        {
            await ConfirmDeleteCoupon.ShowAsync(Coupon);
        }

        public async Task DeleteCouponAsync(MProductCoupon Coupon)
        {
            HttpResponseMessage response = await HttpClient.DeleteAsync("api/offers/coupons/" + Coupon.Id);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                ProductCoupons.Remove(Coupon);
                AlertService.ShowAlert("product-coupons-main", $"Successfully removed sale <span class=\"fw-bold\">{Coupon.CouponName}</span>!", AlertType.Success);
            }
            else
            {
                AlertService.ShowAlert("product-coupons-main", $"There was an error when trying to remove <span class=\"fw-bold\">{Coupon.CouponName}</span>", AlertType.Danger);
            }
        }
    }
}
