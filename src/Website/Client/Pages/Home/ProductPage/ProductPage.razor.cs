using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Extensions;
using Website.Client.Pages.Home.ProductPage.Components;
using Website.Client.Providers;
using Website.Client.Services;
using Website.Components.Helpers;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Client.Pages.Home.ProductPage
{
    public partial class ProductPage
    {
        [Parameter]
        public int ProductId { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        [Inject]
        public CartService CartService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public AuthenticationStateProvider AuthState { get; set; }

        public SteamAuthProvider SteamAuth => AuthState as SteamAuthProvider;

        public MProduct Product { get; set; }

        private MBranch DefaultBranch => Product.Branches.FirstOrDefault(x => x.Versions.Count > 0);
        private MVersion LatestVersion => DefaultBranch?.Versions.OrderByDescending(x => x.CreateDate).FirstOrDefault() ?? null;

        private bool ShowVersions = false;
        private void ToggleShowVersions()
        {
            ShowVersions = !ShowVersions;
        }

        private HttpStatusCode statusCode;

        public ProductReviewModal ReviewModal { get; set; }
        public MProductReview Review { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync("api/products/" + ProductId);
            statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.OK)
            {
                Product = await response.Content.ReadFromJsonAsync<MProduct>();

                if (SteamAuth.IsAuthenticated)
                {
                    Review = Product.Reviews.FirstOrDefault(x => x.UserId == SteamAuth.User.Id);
                    Product.Reviews.Remove(Review);
                }

                foreach (MProductTab tab in Product.Tabs)
                {
                    tab.Content = MarkdownHelper.ParseToHtml(tab.Content);
                }

                Product.RatingsCount = Product.Reviews.Count;
                if (Product.RatingsCount > 0)
                {
                    Product.AverageRating = (byte)(Product.Reviews.Sum(x => x.Rating) / Product.Reviews.Count);
                }
                
            }

            await CartService.ReloadCartAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.RefreshHighlightAsync();
        }

        private bool IsInCart => CartService?.Carts.Exists(x => x.Items.Exists(x => x.ProductId == ProductId)) ?? false;
        private bool IsCustomer => Product.Price <= 0 || Product.Customer != null || IsSeller;
        private bool IsSeller => SteamAuth.IsAuthenticated && SteamAuth.User.Id == Product.SellerId;

        private async Task AddToCartAsync()
        {
            if (IsInCart)
                return;

            var orderItem = new OrderItemParams() 
            { 
                ProductId = ProductId,
                Product = Product
            };

            await CartService.AddToCartAsync(orderItem);
            StateHasChanged();
            NavigationManager.NavigateTo("/cart");
        }

        private async Task ShowReviewModalAsync()
        {
            await ReviewModal.ShowModalAsync(Review);
        }

        private void OnReviewChanged(MProductReview review)
        {
            Review = review;
        }

        private async Task DeleteReviewAsync(MProductReview review)
        {
            await HttpClient.DeleteAsync("api/products/reviews/" + review.Id);
            Product.Reviews.Remove(review);
        }
    }
}
