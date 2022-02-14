using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Services;
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

        private IEnumerable<MProduct> SearchedProducts => Products
            .Where(x => string.IsNullOrEmpty(searchCategory) || x.Category == searchCategory)
            .Where(x => string.IsNullOrEmpty(searchString)
            || x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
            || x.Seller.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
            || x.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase));

        private IEnumerable<MProduct> OrderedProducts
        {
            get
            {
                switch (orderBy)
                {
                    case EOrderBy.MostDownloads:
                        return SearchedProducts.OrderByDescending(x => x.TotalDownloadsCount + x.ServersCount);
                    case EOrderBy.BestRated:
                        return SearchedProducts.OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.RatingsCount);
                    case EOrderBy.PriceAsc:
                        return SearchedProducts.OrderBy(x => x.Price);
                    case EOrderBy.PriceDesc:
                        return SearchedProducts.OrderByDescending(x => x.Price);
                    default:
                        return SearchedProducts.OrderByDescending(x => x.CreateDate);
                }
            }
        }

        private string searchString = string.Empty;
        private string searchCategory = string.Empty;

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
