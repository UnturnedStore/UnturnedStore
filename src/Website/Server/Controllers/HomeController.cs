using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Extensions;

namespace Website.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly HomeRepository homeRepository;

        public HomeController(IConfiguration configuration, HomeRepository homeRepository)
        {
            this.configuration = configuration;
            this.homeRepository = homeRepository;
        }

        [HttpGet("~/discord")]
        public IActionResult Discord()
        {
            return Redirect(configuration["DiscordInviteURL"]);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatisticsAsync()
        {
            return Ok(await homeRepository.GetHomeStatisticsAsync());
        }

        [HttpGet("promoted")]
        public async Task<IActionResult> GetPromotedAsync()
        {
            if (User.TryGetId(out int userId))
            {
                return Ok(await homeRepository.GetPromotedProductsAsync(userId));
            }

            return NotFound();
        }
    }
}
