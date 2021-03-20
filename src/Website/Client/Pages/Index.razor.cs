using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Client.Pages
{
    public partial class Index
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public IEnumerable<ProductModel> Products { get; set; }

        private IEnumerable<ProductModel> SearchedProducts => Products.Where(x => string.IsNullOrEmpty(searchString) 
            || x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
            || x.Seller.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));

        private IEnumerable<ProductModel> OrderedProducts {
            get
            {
                switch (orderBy)
                {
                    case EOrderBy.MostDownloads:
                        return SearchedProducts.OrderByDescending(x => x.TotalDownloadsCount);
                    default:
                        return SearchedProducts.OrderByDescending(x => x.CreateDate);
                }
            }
        }

        private string searchString = string.Empty;


        private bool isSuccessPayment;

        protected override async Task OnInitializedAsync()
        {
            if (QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query).TryGetValue("isSuccessPayment", out var value))
            {
                if (value == "1")
                {
                    isSuccessPayment = true;
                }
            }
            Products = await HttpClient.GetFromJsonAsync<ProductModel[]>("api/products");
        }

        private EOrderBy orderBy = EOrderBy.Newest;

        public enum EOrderBy
        {
            Newest,
            MostDownloads
        }
    }
}
