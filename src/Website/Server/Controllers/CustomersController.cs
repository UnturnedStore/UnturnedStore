using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ProductsRepository productsRepository;
        private readonly CustomersRepository customersRepository;

        public CustomersController(ProductsRepository productsRepository, CustomersRepository customersRepository)
        {
            this.productsRepository = productsRepository;
            this.customersRepository = customersRepository;
        }

        [HttpGet("{customerId}/servers")]
        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        public async Task<IActionResult> GetCustomerServersByCustomerIdAsync([FromRoute] int customerId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductCustomerSellerAsync(customerId, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            IEnumerable<MCustomerServer> customerServers = await customersRepository.GetCustomerServersByCustomerIdAsync(customerId);

            return Ok(customerServers);
        }
    }
}
