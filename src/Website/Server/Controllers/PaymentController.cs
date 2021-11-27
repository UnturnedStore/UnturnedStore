using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Payments.Abstractions;
using Website.Payments.Constants;
using Website.Payments.Providers;
using Website.Payments.Services;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentsRepository paymentsRepository;
        private readonly UsersRepository usersRepository;
        private readonly OrdersRepository ordersRepository;
        private readonly IPaymentProviders paymentProviders;

        public PaymentController(PaymentsRepository paymentsRepository, OrdersRepository ordersRepository,
            UsersRepository usersRepository, IPaymentProviders paymentProviders)
        {
            this.paymentsRepository = paymentsRepository;
            this.ordersRepository = ordersRepository;
            this.usersRepository = usersRepository;
            this.paymentProviders = paymentProviders;
        }

        [HttpGet("{sellerId}")]
        public async Task<IActionResult> GetPaymentMethodsAsync(int sellerId)
        {
            MUser user = await usersRepository.GetUserPrivateAsync(sellerId);
            if (!RoleConstants.IsSeller(user.Role))
                return BadRequest();

            List<string> paymentMethods = new();
            if (user.IsPayPalEnabled)
                paymentMethods.Add(OrderConstants.Methods.PayPal);
            if (user.IsNanoEnabled)
                paymentMethods.Add(OrderConstants.Methods.Nano);

            return Ok(paymentMethods);
        }


        [HttpGet("{orderId}/nano")]
        public async Task<IActionResult> GetNanoPaymentAsync(int orderId)
        {
            MOrder order = await ordersRepository.GetOrderAsync(orderId);

            if (order.PaymentMethod != PaymentConstants.Providers.Nano.Name)
            {
                return BadRequest();
            }

            NanoPaymentProvider provider = paymentProviders.Get<NanoPaymentProvider>();
            return Ok(await provider.CreatePaymentAsync(order));
        }
    }
}
