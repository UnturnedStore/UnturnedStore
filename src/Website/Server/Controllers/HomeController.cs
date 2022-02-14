using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;

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
        public async Task<IActionResult> Statistics()
        {
            return Ok(await homeRepository.GetStatisticsAsync());
        }
    }
}
