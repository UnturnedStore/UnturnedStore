using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
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

        public PaymentController(PaymentsRepository paymentsRepository, UsersRepository usersRepository)
        {
            this.paymentsRepository = paymentsRepository;
            this.usersRepository = usersRepository;
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
    }
}
