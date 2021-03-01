using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public SellerController(SellersRepository sellersRepository)
        {
            this.sellersRepository = sellersRepository;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProductsAsync()
        {
            return Ok(await sellersRepository.GetProductsAsync(int.Parse(User.Identity.Name)));
        }

        [HttpGet("products/{productId}")]
        public async Task<IActionResult> GetProductAsync(int productId)
        {
            return Ok(await sellersRepository.GetProductAsync(productId));
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomersAsync()
        {
            return Ok(await sellersRepository.GetCustomersAsync(int.Parse(User.Identity.Name)));
        }
    }
}
