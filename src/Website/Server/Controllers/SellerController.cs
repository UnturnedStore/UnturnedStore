using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;

namespace Website.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    public class SellerController : ControllerBase
    {
        private readonly SellersRepository sellersRepository;
        private readonly ProductsRepository productsRepository;

        public SellerController(SellersRepository sellersRepository, ProductsRepository productsRepository)
        {
            this.sellersRepository = sellersRepository;
            this.productsRepository = productsRepository;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrdersAsync()
        {
            return Ok(await sellersRepository.GetOrdersAsync(int.Parse(User.Identity.Name)));
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProductsAsync()
        {
            return Ok(await sellersRepository.GetProductsAsync(int.Parse(User.Identity.Name)));
        }

        [HttpGet("products/{productId}")]
        public async Task<IActionResult> GetProductAsync(int productId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductSellerAsync(productId, int.Parse(User.Identity.Name)))
            {
                return BadRequest();
            }

            return Ok(await sellersRepository.GetSellerProductAsync(productId));
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomersAsync()
        {
            return Ok(await sellersRepository.GetCustomersAsync(int.Parse(User.Identity.Name)));
        }
    }
}
