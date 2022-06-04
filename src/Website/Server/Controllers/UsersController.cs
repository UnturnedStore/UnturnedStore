using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models;
using Website.Shared.Models.Database;

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
        public async Task<IActionResult> GetUserAsync(int userId)
        {
            return Ok(await usersRepository.GetUserAsync<UserInfo>(userId));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMeUserAsync()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return Ok(await usersRepository.GetUserAsync<UserInfo>(int.Parse(User.Identity.Name)));
            }
            else
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
        }

        [Authorize]
        [HttpGet("settings")]
        public async Task<IActionResult> GetSettingAsync()
        {
            return Ok(await usersRepository.GetUserAsync(int.Parse(User.Identity.Name)));
        }

        [HttpGet("{userId}/avatar")]
        public async Task<IActionResult> GetUserAvatarAsync(int userId)
        {
            int imageId = await usersRepository.GetUserAvatarImageIdAsync(userId);
            return Redirect($"/api/images/{imageId}");
        }

        [HttpGet("{userId}/profile")]
        public async Task<IActionResult> GetUserProfileAsync(int userId)
        {
            return Ok(await usersRepository.GetUserProfileAsync(userId));
        }

        [HttpGet("{userId}/seller")]
        public async Task<IActionResult> GetUserSellerAsync(int userId)
        {
            Seller seller = await usersRepository.GetUserAsync<Seller>(userId);
            if (RoleConstants.IsNotSeller(seller.Role))
            {
                return BadRequest();
            }

            return Ok(seller);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> PutProfileAsync([FromBody] MUser user)
        {
            if (user.Id.ToString() != User.Identity.Name)
            {
                return Unauthorized();
            }

            await usersRepository.UpdateProfileAsync(user);
            return Ok();
        }

        [Authorize]
        [HttpPut("seller")]
        public async Task<IActionResult> PutSellerAsync([FromBody] MUser user)
        {
            if (user.Id.ToString() != User.Identity.Name)
            {
                return Unauthorized();
            }

            await usersRepository.UpdateSellerAsync(user);
            return Ok();
        }

        [Authorize]
        [HttpPut("notifications")]
        public async Task<IActionResult> PutNotificationsAsync([FromBody] MUser user)
        {
            if (user.Id.ToString() != User.Identity.Name)
            {
                return Unauthorized();
            }

            await usersRepository.UpdateNotificationsAsync(user);
            return Ok();
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [HttpGet("~/signin"), HttpPost("~/signin")]
        public IActionResult SignIn([FromQuery] string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties 
            { 
                RedirectUri = returnUrl,
                IsPersistent = true,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            }, "Steam");
        }

        [ResponseCache(NoStore = true, Duration = 0)]
        [HttpGet("~/signout"), HttpPost("~/signout")]
        public IActionResult LogOut([FromQuery] string returnUrl = "/")
        {
            return SignOut(new AuthenticationProperties 
            {
                RedirectUri = returnUrl,
            }, CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
