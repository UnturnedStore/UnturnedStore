using AngleSharp.Io;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Models.Database;

namespace Website.Server.Pages.Prerender.PrerenderedPages
{
    public class PrerenderedProductPage : IPrerenderedPage
    {
        private readonly ProductsRepository productsRepository;
        public PrerenderedProductPage(ProductsRepository productsRepository)
        {
            this.productsRepository = productsRepository;
        }

        private int ProductId;
        public bool IsPage(HttpRequest Request)
        {
            return Request.Path.StartsWithSegments("/products") && Request.Path.Value.Length >= 10 && int.TryParse(Request.Path.Value.AsSpan(10), out ProductId);
        }

        public async Task<PrerenderedMetaTags> GetMetaTags()
        {
            MProduct product = await productsRepository.GetProductAsync(ProductId, 0);
            if (product == null) return new PrerenderedMetaTags();
            return new PrerenderedMetaTags(product);
        }
    }
}
