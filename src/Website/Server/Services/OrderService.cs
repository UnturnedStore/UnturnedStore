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
        private readonly PayPalService payPalService;

        public OrderService(OrdersRepository ordersRepository, ProductsRepository productsRepository, UsersRepository usersRepository, PayPalService payPalService)
        {
            this.ordersRepository = ordersRepository;
            this.productsRepository = productsRepository;
            this.usersRepository = usersRepository;
            this.payPalService = payPalService;
        }

        public async Task<MOrder> CreateOrderAsync(OrderParams orderParams)
        {
            if (orderParams.PaymentMethod == PaymentContants.PayPal)
            {
                var order = MOrder.FromParams(orderParams);

                order.Seller = await usersRepository.GetUserPrivateAsync(order.SellerId);

                order.PaymentReceiver = order.Seller.PayPalEmail;
                order.Currency = order.Seller.PayPalCurrency;
                order.Status = PaymentContants.PendingStatus;

                foreach (var itemParams in orderParams.Items)
                {
                    var item = MOrderItem.FromParams(itemParams);

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
                payPalService.PayPalPayment(order, orderParams.BaseUrl);
                await ordersRepository.UpdateOrderPaymentUrlAsync(order);
                return order;
            }

            return null;
        }
    }
}
