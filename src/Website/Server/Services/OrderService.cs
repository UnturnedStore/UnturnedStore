using Microsoft.Extensions.Logging;
using RestoreMonarchy.PaymentGateway.Client;
using RestoreMonarchy.PaymentGateway.Client.Models;
using System;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Server.Services
{
    public class OrderService
    {
        private readonly OrdersRepository ordersRepository;
        private readonly ProductsRepository productsRepository;
        private readonly OffersRepository offersRepository;
        private readonly UsersRepository usersRepository;
        private readonly ILogger<OrderService> logger;
        private readonly DiscordService discordService;

        public OrderService(OrdersRepository ordersRepository, ProductsRepository productsRepository, OffersRepository offersRepository, 
            UsersRepository usersRepository, ILogger<OrderService> logger, DiscordService discordService, PaymentGatewayClient paymentGatewayClient)
        {
            this.ordersRepository = ordersRepository;
            this.productsRepository = productsRepository;
            this.offersRepository = offersRepository;
            this.usersRepository = usersRepository;
            this.logger = logger;
            this.discordService = discordService;

            PaymentGatewayClient = paymentGatewayClient;
        }

        public PaymentGatewayClient PaymentGatewayClient { get; }

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

            foreach (MOrderItem orderItem in order.Items)
            {
                await productsRepository.AddProductCustomerAsync(new MProductCustomer()
                {
                    UserId = order.BuyerId,
                    ProductId = orderItem.ProductId
                });
            }

            discordService.SendPurchaseNotification(order);
            logger.LogInformation($"Successfully finished the order #{order.Id}");
        }

        public async Task<MOrder> CreateOrderAsync(OrderParams orderParams)
        {
            MOrder order = MOrder.FromParams(orderParams);
            order.Seller = await usersRepository.GetUserAsync<Seller>(order.SellerId);
            order.Status = OrderConstants.Status.Pending;
            order.Currency = "USD";
            order.PaymentMethod = orderParams.PaymentMethod;
            order.PaymentReceiver = order.GetReceiver(order.PaymentMethod);

            foreach (OrderItemParams orderItemParams in orderParams.Items)
            {
                MOrderItem orderItem = MOrderItem.FromParams(orderItemParams);
                orderItem.Product = await productsRepository.GetProductAsync(orderItem.ProductId, order.BuyerId);
                if (orderItem.Product == null)
                {
                    return null;
                }

                if (order.SellerId != orderItem.Product.SellerId)
                {
                    return null;
                }

                if (orderItem.Product.Customer != null)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(orderItem.CouponCode))
                {
                    orderItem.Coupon = await offersRepository.GetCouponFromCodeAsync(orderItem.CouponCode, orderItem.ProductId);
                    
                    if (orderItem.Coupon == null)
                    {
                        return null;
                    }

                    orderItem.CouponId = orderItem.Coupon.Id;
                    orderItem.CouponMultiplier = orderItem.Coupon.CouponMultiplier;
                }

                orderItem.ProductName = orderItem.Product.Name;
                orderItem.Price = orderItem.Product.DiscountedPrice(orderItem.Coupon);
                if (orderItem.Product.Sale != null) orderItem.SaleId = orderItem.Product.Sale.Id;

                order.TotalPrice += orderItem.Price;
                order.Items.Add(orderItem);
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
