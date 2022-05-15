using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly UsersRepository usersRepository;
        private readonly IConfiguration configuration;

        public PaymentsController(UsersRepository usersRepository, IConfiguration configuration)
        {
            this.usersRepository = usersRepository;
            this.configuration = configuration;
        }

        [HttpGet("{sellerId}")]
        public async Task<IActionResult> GetPaymentMethodsAsync(int sellerId)
        {
            MUser user = await usersRepository.GetUserPrivateAsync(sellerId);
            if (RoleConstants.IsNotSeller(user.Role))
            {
                return BadRequest();
            }

            return Ok(user.GetSellerPaymentProviders(configuration.GetValue<bool>("IsMockEnabled")));
        }
    }
}
