using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.Home.IndexPage
{
    public partial class Index
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public StorageService StorageService { get; set; }

        public IEnumerable<MProduct> Products { get; set; }

        private IEnumerable<MProduct> SearchedProducts => Products
            .Where(x => string.IsNullOrEmpty(searchCategory) || x.Category == searchCategory)
            .Where(x => string.IsNullOrEmpty(searchString)
            || x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
            || x.Seller.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));

        private IEnumerable<MProduct> OrderedProducts
        {
            get
            {
                switch (orderBy)
                {
                    case EOrderBy.MostDownloads:
                        return SearchedProducts.OrderByDescending(x => x.TotalDownloadsCount);
                    case EOrderBy.BestRated:
                        return SearchedProducts.OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.RatingsCount);
                    case EOrderBy.PriceAsc:
                        return SearchedProducts.OrderBy(x => x.DiscountedPrice());
                    case EOrderBy.PriceDesc:
                        return SearchedProducts.OrderByDescending(x => x.DiscountedPrice());
                    default:
                        return SearchedProducts.OrderByDescending(x => x.ReleaseDate is null ? x.CreateDate : x.ReleaseDate);
                }
            }
        }

        private string searchString = string.Empty;
        private string searchCategory = string.Empty;

        private void ChangeCategory(string category)
        {
            searchCategory = category;
        }

        private string Active(string category)
        {
            if (searchCategory == category)
                return "active";
            return string.Empty;
        }

        private string GetCategoryIcon()
        {
            return ProductCategoryConstants.GetCategoryIcon(searchCategory);
        }

        protected override async Task OnInitializedAsync()
        {
            Products = await HttpClient.GetFromJsonAsync<MProduct[]>("api/products");
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