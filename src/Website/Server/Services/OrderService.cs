using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestoreMonarchy.PaymentGateway.Client;
using System;
using System.Threading.Tasks;
using RestoreMonarchy.PaymentGateway.Client.Models;
using Website.Data.Repositories;
using Website.Server.Options;
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

        public PaymentGatewayClient PaymentGatewayClient { get; }

        public OrderService(OrdersRepository ordersRepository, ProductsRepository productsRepository, UsersRepository usersRepository, 
            ILogger<OrderService> logger, DiscordService discordService, PaymentGatewayClient paymentGatewayClient)
        {
            this.ordersRepository = ordersRepository;
            this.productsRepository = productsRepository;
            this.usersRepository = usersRepository;
            this.logger = logger;
            this.discordService = discordService;

            PaymentGatewayClient = paymentGatewayClient;
        }

        public async Task UpdateOrderAsync(Guid paymentId)
        {
            MOrder order = await ordersRepository.GetOrderAsync(paymentId);

            if (order == null)
            {
                logger.LogWarning("Order for payment id {0} not found", paymentId);
                return;
            }

            if (order.Status == OrderConstants.Status.Completed)
            {
                logger.LogWarning("Order #{0} was already completed", order.Id);
                return;
            }

            order.Status = OrderConstants.Status.Completed;
            order.LastUpdate = DateTime.Now;
            await ordersRepository.UpdateOrderAsync(order);

            foreach (MOrderItem item in order.Items)
            {
                await productsRepository.AddProductCustomerAsync(new MProductCustomer()
                {
                    UserId = order.BuyerId,
                    ProductId = item.ProductId
                });
            }

            discordService.SendPurchaseNotification(order);
            logger.LogInformation($"Successfully finished the order #{order.Id}");
        }

        public async Task<MOrder> CreateOrderAsync(OrderParams orderParams)
        {
            MOrder order = MOrder.FromParams(orderParams);
            order.Seller = await usersRepository.GetUserPrivateAsync(order.SellerId);
            order.Status = OrderConstants.Status.Pending;
            order.Currency = order.Seller.PayPalCurrency;
            order.PaymentMethod = orderParams.PaymentMethod;
            order.PaymentReceiver = order.GetReceiver(order.PaymentMethod);
            

            foreach (var itemParams in orderParams.Items)
            {
                MOrderItem item = MOrderItem.FromParams(itemParams);

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

            Payment payment = Payment.Create(order.PaymentMethod, string.Empty, 
                order.GetReceiver(order.PaymentMethod), order.Currency, order.TotalPrice);

            foreach (MOrderItem item in order.Items)
            {
                payment.AddItem(item.ProductName, 1, item.Price);
            }

            order.PaymentId = await PaymentGatewayClient.CreatePaymentAsync(payment);
            return await ordersRepository.AddOrderAsync(order);            
        }
    }
}
