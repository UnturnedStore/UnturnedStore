using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet("discord")]
        public IActionResult Discord()
        {
            return Redirect(configuration["DiscordInviteURL"]);
        }

        [HttpGet("forum")]
        public IActionResult Forum()
        {
            return Redirect(configuration["ForumURL"]);
        }
    }
}
