using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Models;
using Website.Shared.Params;

namespace Website.Client.Services
{
    public class CartService
    {
        private readonly StorageService storageService;
        private readonly HttpClient httpClient;

        public CartService(StorageService storageService, HttpClient httpClient)
        {
            this.storageService = storageService;
            this.httpClient = httpClient;
        }

        public List<OrderParams> Carts { get; private set; }

        public async Task ReloadCartAsync()
        {
            Carts = await storageService.GetSessionItemAsync<List<OrderParams>>("carts");
            if (Carts == null)
            {
                Carts = new List<OrderParams>();
            }

            foreach (var orderParams in Carts.ToList())
            {
                orderParams.Seller = await httpClient.GetFromJsonAsync<UserModel>("api/users/" + orderParams.SellerId);

                foreach (var item in orderParams.Items.ToList())
                {
                    item.Product = await httpClient.GetFromJsonAsync<ProductModel>("api/products/" + item.ProductId);

                    if (item.Product.Customer != null)
                    {
                        await RemoveFromCartAsync(orderParams, item);
                    }                        
                }
            }
        }

        private async Task SaveCartAsync()
        {
            await storageService.SetSessionItemAsync("carts", Carts);
        }

        public async Task AddToCartAsync(OrderItemParams item)
        {
            if (Carts == null)
                await ReloadCartAsync();

            var orderParams = Carts.FirstOrDefault(x => x.SellerId == item.Product.SellerId);
            
            if (orderParams == null)
            {
                orderParams = new OrderParams()
                {
                    SellerId = item.Product.SellerId,
                    PaymentMethod = PaymentContants.PayPal,
                    Seller = item.Product.Seller,
                    Items = new List<OrderItemParams>()
                };
                Carts.Add(orderParams);
            }

            orderParams.Items.Add(item);            
            await SaveCartAsync();
        }

        public async Task RemoveFromCartAsync(OrderParams orderParams, OrderItemParams item)
        {
            if (Carts == null)
                await ReloadCartAsync();

            orderParams.Items.Remove(item);
            if (orderParams.Items.Count == 0)
            {
                Carts.Remove(orderParams);
            }

            await SaveCartAsync();
        }

        public async Task RemoveCartAsync(OrderParams orderParams)
        {
            if (Carts == null)
                await ReloadCartAsync();

            if (Carts.Remove(orderParams))
                await SaveCartAsync();
        }
    }
}
