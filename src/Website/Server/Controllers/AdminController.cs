using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Server.Controllers
{
    [Authorize(Roles = RoleConstants.AdminRoleId)]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AdminRepository adminRepository;

        public AdminController(AdminRepository adminRepository)
        {
            this.adminRepository = adminRepository;
        }

        [HttpPut("users")]
        public async Task<IActionResult> UpdateUserAsync([FromBody] MUser user)
        {
            await adminRepository.UpdateUserAsync(user);
            return Ok();
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProductsAsync()
        {
            return Ok(await adminRepository.GetProductsAsync());
        }
    }
}
