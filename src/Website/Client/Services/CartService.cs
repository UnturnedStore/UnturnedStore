using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Shared;
using Website.Shared.Constants;
using Website.Shared.Models;
using Website.Shared.Params;

namespace Website.Client.Services
{
    public class CartService
    {
        private readonly StorageService storageService;
        private readonly HttpClient httpClient;

        private NavMenu NavMenu { get; set; }
        public void SetNavMenu(NavMenu navMenu)
        {
            NavMenu = navMenu;
        }

        public CartService(StorageService storageService, HttpClient httpClient)
        {
            this.storageService = storageService;
            this.httpClient = httpClient;
        }

        public List<OrderParams> Carts { get; private set; }

        private bool wasReloaded = false;

        public async Task ReloadCartAsync(bool onlyIfNull = false)
        {
            if (onlyIfNull && wasReloaded)
                return;

            Carts = await storageService.GetSessionItemAsync<List<OrderParams>>("carts");
            if (Carts == null)
            {
                Carts = new List<OrderParams>();
            }

            foreach (var orderParams in Carts.ToList())
            {
                orderParams.Seller = await httpClient.GetFromJsonAsync<MUser>($"api/users/{orderParams.SellerId}/seller");

                foreach (var item in orderParams.Items.ToList())
                {
                    item.Product = await httpClient.GetFromJsonAsync<MProduct>("api/products/" + item.ProductId);

                    if (item.Product.Customer != null)
                    {
                        await RemoveFromCartAsync(orderParams, item);
                    }                        
                }
            }

            wasReloaded = true;
            if (NavMenu != null)
                NavMenu.Refresh();
        }

        public async Task UpdateCartAsync()
        {            
            await storageService.SetSessionItemAsync("carts", Carts);
            if (NavMenu != null)
                NavMenu.Refresh();
        }

        public async Task<OrderParams> GetOrderParamsAsync(int sellerId)
        {
            if (Carts == null)
                await ReloadCartAsync();

            return Carts.FirstOrDefault(x => x.SellerId == sellerId);
        }

        public async Task AddToCartAsync(OrderItemParams item)
        {
            if (Carts == null)
                await ReloadCartAsync();

            OrderParams orderParams = Carts.FirstOrDefault(x => x.SellerId == item.Product.SellerId);
            
            if (orderParams == null)
            {
                orderParams = new OrderParams()
                {
                    SellerId = item.Product.SellerId,
                    PaymentMethod = OrderConstants.Methods.PayPal,
                    Seller = item.Product.Seller,
                    Items = new List<OrderItemParams>()
                };
                Carts.Add(orderParams);
            }

            orderParams.Items.Add(item);            
            await UpdateCartAsync();
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

            await UpdateCartAsync();
        }

        public async Task RemoveCartAsync(OrderParams orderParams)
        {
            if (Carts == null)
                await ReloadCartAsync();

            if (Carts.Remove(orderParams))
                await UpdateCartAsync();
        }
    }
}
