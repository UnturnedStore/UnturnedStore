using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models;
using Website.Shared.Params;

namespace Website.Server.Services
{
    public class OrderService
    {
        private readonly OrdersRepository ordersRepository;
        private readonly ProductsRepository productsRepository;
        private readonly UsersRepository usersRepository;

        public OrderService(OrdersRepository ordersRepository, ProductsRepository productsRepository, UsersRepository usersRepository)
        {
            this.ordersRepository = ordersRepository;
            this.productsRepository = productsRepository;
            this.usersRepository = usersRepository;
        }

        public async Task<OrderModel> CreateOrderAsync(OrderParams orderParams)
        {
            if (orderParams.PaymentMethod == PaymentContants.PayPal)
            {
                var order = OrderModel.FromParams(orderParams);

                order.Seller = await usersRepository.GetUserAsync(order.SellerId);

                order.PaymentReceiver = order.Seller.PayPalEmail;
                order.Currency = order.Seller.PayPalCurrency;
                order.Status = PaymentContants.PendingStatus;

                foreach (var itemParams in orderParams.Items)
                {
                    var item = OrderItemModel.FromParams(itemParams);

                    item.Product = await productsRepository.GetProductAsync(item.ProductId, order.BuyerId);
                    
                    if (item.Product == null)
                        return null;

                    if (order.SellerId != item.Product.SellerId)
                        return null;

                    if (item.Product.Customer != null)
                        return null;                    

                    item.ProductName = item.Product.Name;
                    item.Price = item.Product.Price;
                    
                    order.TotalPrice += item.Price;
                    order.Items.Add(item);
                }

                order = await ordersRepository.AddOrderAsync(order);
                PayPalService.PayPalPayment(order, orderParams.BaseUrl);
                await ordersRepository.UpdateOrderPaymentUrlAsync(order);
                return order;
            }

            return null;
        }

        public async Task CompleteOrderAsync(OrderModel order)
        {
            foreach (var item in order.Items)
            {
                await productsRepository.AddProductCustomerAsync(new ProductCustomerModel()
                {
                    UserId = order.BuyerId,
                    ProductId = item.ProductId
                });
            }
        }
    }
}
