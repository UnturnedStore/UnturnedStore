using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Server.Services;
using Website.Shared.Constants;
using Website.Shared.Models;
using Website.Shared.Params;

namespace Website.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersRepository ordersRepository;
        private readonly OrderService orderService;
        private readonly PayPalService paypalService;

        public OrdersController(OrdersRepository ordersRepository, OrderService orderService, PayPalService paypalService)
        {
            this.ordersRepository = ordersRepository;
            this.orderService = orderService;
            this.paypalService = paypalService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostOrderAsync([FromBody] OrderParams orderParams)
        {
            orderParams.BaseUrl = Request.Headers["Origin"];
            orderParams.BuyerId = int.Parse(User.Identity.Name);
            var order = await orderService.CreateOrderAsync(orderParams);
            if (order == null)
                return BadRequest();

            return Ok(order);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetOrdersAsync()
        {
            return Ok(await ordersRepository.GetOrdersAsync(int.Parse(User.Identity.Name)));
        }
        
        [HttpPost(PaymentContants.PayPal)]
        public async Task<IActionResult> PayPalAsync()
        {
            await paypalService.ProcessPaymentAsync(Request);
            return Ok();
        }
    }
}
