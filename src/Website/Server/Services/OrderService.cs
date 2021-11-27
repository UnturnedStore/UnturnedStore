using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Payments;
using Website.Payments.Abstractions;
using Website.Payments.Constants;
using Website.Payments.Models;
using Website.Payments.Providers;
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
        private readonly ILogger<OrderService> logger;
        private readonly DiscordService discordService;
        private readonly IPaymentProviders paymentProviders;

        public OrderService(OrdersRepository ordersRepository, ProductsRepository productsRepository, UsersRepository usersRepository, 
            ILogger<OrderService> logger, DiscordService discordService, IPaymentProviders paymentProviders)
        {
            this.ordersRepository = ordersRepository;
            this.productsRepository = productsRepository;
            this.usersRepository = usersRepository;
            this.logger = logger;
            this.discordService = discordService;
            this.paymentProviders = paymentProviders;            
        }

        public async Task UpdateOrderAsync(ValidatePaymentResult result)
        {
            if (result.Status != PaymentStatus.Valid)
            {
                logger.LogWarning(result.ErrorMessage);
                return;
            }

            await ordersRepository.UpdateOrderAsync(result.Order);

            if (result.Order.Status == OrderConstants.Status.Completed)
            {
                foreach (var item in result.Order.Items)
                {
                    await productsRepository.AddProductCustomerAsync(new MProductCustomer()
                    {
                        UserId = result.Order.BuyerId,
                        ProductId = item.ProductId
                    });
                }

                discordService.SendPurchaseNotification(result.Order);
                logger.LogInformation($"Successfully finished the order #{result.Order.Id}");
            }
        }

        public async Task<MOrder> CreateOrderAsync(OrderParams orderParams)
        {
            MOrder order = MOrder.FromParams(orderParams);
            order.Seller = await usersRepository.GetUserPrivateAsync(order.SellerId);
            order.Status = OrderConstants.Status.Pending;

            if (orderParams.PaymentMethod == PaymentConstants.Providers.PayPal.Name)
            {
                paymentProviders.Get<PayPalPaymentProvider>().BuildOrder(order);
            } else if (orderParams.PaymentMethod == PaymentConstants.Providers.Nano.Name)
            {
                paymentProviders.Get<NanoPaymentProvider>().BuildOrder(order);
            }

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

            return await ordersRepository.AddOrderAsync(order);            
        }
    }
}
