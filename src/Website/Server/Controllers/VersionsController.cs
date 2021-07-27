using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Server.Services;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionsController : ControllerBase
    {
        private readonly VersionsRepository versionsRepository;
        private readonly BranchesRepository branchesRepository;
        private readonly DiscordService discordService;

        public VersionsController(VersionsRepository versionsRepository, BranchesRepository branchesRepository, DiscordService discordService)
        {
            this.versionsRepository = versionsRepository;
            this.branchesRepository = branchesRepository;
            this.discordService = discordService;
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost]
        public async Task<IActionResult> PostVersionAsync([FromBody] MVersion version)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await branchesRepository.IsBranchSellerAsync(version.BranchId, int.Parse(User.Identity.Name)))
                return BadRequest();

            version = await versionsRepository.AddVersionAsync(version);

            await discordService.SendVersionUpdateAsync(version, Request.Headers["Origin"]);

            return Ok(version);
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPut]
        public async Task<IActionResult> PutVersionAsync([FromBody] MVersion version)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await branchesRepository.IsBranchSellerAsync(version.BranchId, int.Parse(User.Identity.Name)))
                return BadRequest();

            await versionsRepository.UpdateVersionAsync(version);

            return Ok();
        }


        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPatch("{versionId}")]
        public async Task<IActionResult> PatchVersionAsync(int versionId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await versionsRepository.IsVersionOwnerAsync(versionId, int.Parse(User.Identity.Name)))
                return BadRequest();

            await versionsRepository.ToggleVersionAsync(versionId);
            return Ok();
        }

        [HttpGet("download/{versionId}")]
        public async Task<IActionResult> DownloadVersionAsync(int versionId)
        {
            int userId = 0;
            if (User.Identity?.IsAuthenticated ?? false)
                userId = int.Parse(User.Identity.Name);

            bool isOwner = await versionsRepository.IsVersionOwnerAsync(versionId, userId);
            var version = await versionsRepository.GetVersionAsync(versionId, isOwner);
            if (!isOwner && version.Branch.Product.Price > 0 && !User.IsInRole(RoleConstants.AdminRoleId))
            {
                if (!await versionsRepository.IsVersionCustomerAsync(versionId, userId))
                    return Unauthorized();
            }

            await versionsRepository.IncrementDownloadsCount(versionId);

            Response.Headers.Add("Content-Disposition", "inline; filename=" + 
                string.Concat(version.Branch.Product.Name, "-", version.Branch.Name, "-", version.Name, ".zip"));
            return File(version.Content, version.ContentType);
        }
    }
}
