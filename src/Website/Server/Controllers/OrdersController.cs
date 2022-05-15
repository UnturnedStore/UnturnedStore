using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Server.Services;
using Website.Shared.Extensions;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersRepository ordersRepository;
        private readonly OrderService orderService;

        public OrdersController(OrdersRepository ordersRepository, OrderService orderService)
        {
            this.ordersRepository = ordersRepository;
            this.orderService = orderService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostOrderAsync([FromBody] OrderParams orderParams)
        {
            orderParams.BaseUrl = Request.Headers["Origin"];
            orderParams.BuyerId = User.Id();

            MOrder order = await orderService.CreateOrderAsync(orderParams);
            if (order == null)
            {
                return BadRequest();
            }

            return Ok(order);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetOrdersAsync()
        {
            return Ok(await ordersRepository.GetOrdersAsync(User.Id()));
        }

        [Authorize]
        [HttpGet("{orderId}/pay")]
        public async Task<IActionResult> PayOrderAsync(int orderId)
        {
            MOrder order = await ordersRepository.GetOrderAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            if (order.BuyerId != User.Id())
            {
                return BadRequest();
            }

            return Redirect(orderService.PaymentGatewayClient.BuildPayUrl(order.PaymentId));
        }

        [HttpPost("Notify")]
        public async Task<IActionResult> NotifyAsync()
        {
            if (!orderService.PaymentGatewayClient.ValdiateNotifyRequest(Request))
            {
                return Forbid();
            }

            string requestBody;
            using (StreamReader reader = new(Request.Body, Encoding.ASCII))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            if (!Guid.TryParse(requestBody, out Guid paymentId))
            {
                return BadRequest();
            }

            await orderService.UpdateOrderAsync(paymentId);
            return Ok();
        }
    }
}
