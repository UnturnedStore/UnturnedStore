using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Extensions;
using Website.Shared.Models;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Seller.CustomersPage.Components
{
    public partial class AddCustomerModal
    {
        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        [Inject]
        public HttpClient HttpClient { get; set; }

        [Parameter]
        public List<MProductCustomer> Customers { get; set; }
        [Parameter]
        public EventCallback<MProductCustomer> OnCustomerAdded { get; set; }

        public IEnumerable<MProduct> Products { get; set; }
        public IEnumerable<UserInfo> Users { get; set; }

        private IEnumerable<UserInfo> SearchedUsers => Users.Where(x => x.Name.Contains(searchUser, StringComparison.OrdinalIgnoreCase) || x.SteamId == searchUser).Take(3);
        private IEnumerable<MProduct> SearchedProducts => Products.Where(x => x.Price > 0 && x.Name.Contains(searchProduct, StringComparison.OrdinalIgnoreCase)).Take(3);

        private string searchProduct = string.Empty;
        private string searchUser = string.Empty;

        private void ResetSearchProduct()
        {
            searchProduct = string.Empty;
        }

        private void ResetSearchUser()
        {
            searchUser = string.Empty;
        }

        private void ResetSearches()
        {
            ResetSearchProduct();
            ResetSearchUser();
        }

        public MProductCustomer Model { get; set; }

        private MProductCustomer DefaultModel => new MProductCustomer();

        protected override async Task OnInitializedAsync()
        {
            Products = await HttpClient.GetFromJsonAsync<MProduct[]>("api/seller/products");
            Users = await HttpClient.GetFromJsonAsync<UserInfo[]>("api/users");

            Model = DefaultModel;
        }

        public async Task ShowAsync()
        {
            msg = null;
            await JsRuntime.ShowModalStaticAsync(nameof(AddCustomerModal));
        }

        private string msg = null;

        private bool isLoading = false;
        public async Task SubmitAsync()
        {
            if (Customers.Exists(x => x.UserId == Model.UserId && x.ProductId == Model.ProductId))
            {
                msg = $"{Model.User.Name} already has {Model.Product.Name}";
                return;
            }
            else
            {
                msg = null;
            }

            isLoading = true;

            var response = await HttpClient.PostAsJsonAsync("api/products/customers", Model);
            var customer = await response.Content.ReadFromJsonAsync<MProductCustomer>();
            customer.Product = Model.Product;
            customer.User = Model.User;

            await OnCustomerAdded.InvokeAsync(customer);

            isLoading = false;

            ResetSearches();
            Model = DefaultModel;
            await JsRuntime.HideModalAsync(nameof(AddCustomerModal));
        }

        private void ChangeProduct(MProduct product)
        {
            Model.Product = product;
            Model.ProductId = product.Id;
            ResetSearchProduct();
        }

        private void ChangeUser(UserInfo user)
        {
            Model.User = user;
            Model.UserId = user.Id;
            ResetSearchUser();
        }

        private void ResetProduct()
        {
            Model.Product = null;
        }

        private void ResetUser()
        {
            Model.User = null;
        }
    }
}
