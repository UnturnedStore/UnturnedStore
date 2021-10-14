using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Payments.Abstractions;
using Website.Payments.Models;
using Website.Payments.Providers;
using Website.Server.Services;
using Website.Shared.Extensions;
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
        private readonly IPaymentProviders paymentProviders;

        public OrdersController(OrdersRepository ordersRepository, OrderService orderService, IPaymentProviders paymentProviders)
        {
            this.ordersRepository = ordersRepository;
            this.orderService = orderService;
            this.paymentProviders = paymentProviders;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostOrderAsync([FromBody] OrderParams orderParams)
        {
            orderParams.BaseUrl = Request.Headers["Origin"];
            orderParams.BuyerId = User.Id();

            var order = await orderService.CreateOrderAsync(orderParams);
            if (order == null)
                return BadRequest();

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
                return NotFound();

            if (order.BuyerId != User.Id())
                return BadRequest();

            return order.PaymentMethod switch
            {
                "PayPal" => Redirect(paymentProviders.Get<PayPalPaymentProvider>().GetPaymentUrl(order, Request.Headers["Origin"])),
                _ => StatusCode((int)HttpStatusCode.ServiceUnavailable),
            };
        }

        [HttpPost("PayPal")]
        public async Task<IActionResult> PayPalAsync()
        {
            PayPalPaymentProvider paypal = paymentProviders.Get<PayPalPaymentProvider>();

            string requestBody;
            using (StreamReader reader = new(Request.Body, Encoding.ASCII))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            ValidatePaymentResult result = await paypal.ValidatePaymentAsync(requestBody);
            await orderService.UpdateOrderAsync(result);
            return Ok();
        }
    }
}
