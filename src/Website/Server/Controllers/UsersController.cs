using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Models;

namespace Website.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersRepository usersRepository;

        public UsersController(UsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync()
        {
            return Ok(await usersRepository.GetUsersAsync());
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProfileAsync(int userId)
        {
            return Ok(await usersRepository.GetUserPublicAsync(userId));
        }

        [HttpPut]
        public async Task<IActionResult> PutUserAsync([FromBody] MUser user)
        {
            await usersRepository.UpdateUserAsync(user);
            return Ok();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMeUserAsync()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return Ok(await usersRepository.GetUserPrivateAsync(int.Parse(User.Identity.Name)));
            }
            else
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [HttpGet("~/signin"), HttpPost("~/signin")]
        public IActionResult SignIn([FromQuery] string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, "Steam");
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [HttpGet("~/signout"), HttpPost("~/signout")]
        public IActionResult LogOut([FromQuery] string returnUrl = "/")
        {
            return SignOut(new AuthenticationProperties { RedirectUri = returnUrl },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
