using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Home.ProductsPage
{
    public partial class ProductsPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public StorageService StorageService { get; set; }

        public IEnumerable<MProduct> Products { get; set; }
        public List<MProductTag> ProductTags { get; set; }

        private decimal HighestProductPrice => Products.OrderByDescending(x => x.Price).FirstOrDefault()?.Price ?? 0;

        private IEnumerable<MProduct> SearchedProducts => Products
            .Where(x => string.IsNullOrEmpty(searchCategory) || x.Category == searchCategory)
            .Where(x => searchTagIds.Count == 0 || searchTagIds.All(t => x.Tags.Contains(t)))
            .Where(x => x.Price >= (minPrice < maxPrice ? minPrice : maxPrice) && x.Price <= (minPrice < maxPrice ? maxPrice : minPrice))
            .Where(x => minRating == 0 || x.AverageRating >= minRating)
            .Where(x => !verifiedSellersOnly || x.Seller.IsVerifiedSeller)
            .Where(x => string.IsNullOrEmpty(searchString)
            || x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
            || x.Seller.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
            || x.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase));

        private List<MProduct> OrderedProducts
        {
            get
            {
                var products = orderBy switch
                {
                    EOrderBy.MostDownloads => SearchedProducts.OrderByDescending(x => x.TotalDownloadsCount + x.ServersCount),
                    EOrderBy.BestRated => SearchedProducts.OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.RatingsCount),
                    EOrderBy.PriceAsc => SearchedProducts.OrderBy(x => x.Price),
                    EOrderBy.PriceDesc => SearchedProducts.OrderByDescending(x => x.Price),
                    _ => SearchedProducts.OrderByDescending(x => x.CreateDate)
                };

                return products.ToList();
            }
        }

        private string searchString = string.Empty;
        private string searchCategory = string.Empty;
        private HashSet<MProductTag> searchTagIds = new HashSet<MProductTag>();
        private decimal minPrice = 0.00M;
        private decimal maxPrice = 0.00M;
        private byte minRating = 0;
        private bool verifiedSellersOnly = false;

        private void HandleSearchTag(MProductTag Tag, bool Value)
        {
            if (!Value && searchTagIds.Contains(Tag)) searchTagIds.Remove(Tag);
            else if (Value && !searchTagIds.Contains(Tag)) searchTagIds.Add(Tag);
        }

        private string minPriceString
        {
            get => minPrice.ToString("F2");
            set
            {
                if (decimal.TryParse(value, out decimal result))
                {
                    minPrice = Math.Round(result, 2);
                }
            }
        }

        private string maxPriceString
        {
            get => maxPrice.ToString("F2");
            set
            {
                if (decimal.TryParse(value, out decimal result))
                {
                    maxPrice = Math.Round(result, 2);
                }
            }
        }

        private bool IsHoveringRatings = false;
        private decimal minRatingHover = 0;

        private void ChangeHoverRating(byte newRating)
        {
            minRatingHover = newRating;
        }

        private void ChangeRating(byte newRating)
        {
            if (newRating == minRating) minRating = 0;
            else minRating = newRating;
        }

        private string GetRatingClass(byte rating)
        {
            if (rating <= (IsHoveringRatings ? minRatingHover : minRating)) return "bi-star-fill";
            return "bi-star";
        }
        
        protected override async Task OnInitializedAsync()
        {
            Products = await HttpClient.GetFromJsonAsync<MProduct[]>("api/products");
            maxPrice = HighestProductPrice;

            ProductTags = await HttpClient.GetFromJsonAsync<List<MProductTag>>("api/products/tags");
        }

        private EOrderBy orderBy = EOrderBy.Newest;

        public enum EOrderBy
        {
            Newest,
            MostDownloads,
            BestRated,
            PriceAsc,
            PriceDesc
        }
    }
}
