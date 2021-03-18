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
    public class PluginsController : ControllerBase
    {
        private readonly VersionsRepository pluginsRepository;
        private readonly BranchesRepository branchesRepository;
        private readonly PluginService pluginService;
        private readonly DiscordService discordService;

        public PluginsController(VersionsRepository pluginsRepository, BranchesRepository branchesRepository, PluginService pluginService, 
            DiscordService discordService)
        {
            this.pluginsRepository = pluginsRepository;
            this.branchesRepository = branchesRepository;
            this.pluginService = pluginService;
            this.discordService = discordService;
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost]
        public async Task<IActionResult> PostPluginAsync([FromBody] VersionModel plugin)
        {
            if (!await branchesRepository.IsBranchSellerAsync(plugin.BranchId, int.Parse(User.Identity.Name)))
                return BadRequest();

            plugin = await pluginsRepository.AddPluginAsync(plugin);

            await discordService.SendPluginUpdateAsync(plugin, Request.Headers["Origin"]);

            return Ok(plugin);
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPatch("{pluginId}")]
        public async Task<IActionResult> PatchPluginAsync(int pluginId)
        {
            if (!await pluginsRepository.IsVersionOwnerAsync(pluginId, int.Parse(User.Identity.Name)))
                return BadRequest();

            await pluginsRepository.TogglePluginAsync(pluginId);
            return Ok();
        }

        [HttpGet("{pluginId}/zip")]
        public async Task<IActionResult> GetPluginZipAsync(int pluginId)
        {
            int userId = 0;
            if (User.Identity?.IsAuthenticated ?? false)
                userId = int.Parse(User.Identity.Name);

            var plugin = await pluginsRepository.GetPluginAsync(pluginId, await pluginsRepository.IsVersionOwnerAsync(pluginId, userId));
            if (plugin.Branch.Product.Price > 0)
            {
                if (!await pluginsRepository.IsVersionCustomerAsync(pluginId, userId))
                    return Unauthorized();
            }

            await pluginsRepository.IncrementDownloadsCount(pluginId);

            Response.Headers.Add("Content-Disposition", "inline; filename=" + 
                string.Concat(plugin.Branch.Product.Name, "-", plugin.Branch.Name, "-", plugin.Name, ".zip"));
            return File(pluginService.ZipPlugin(plugin), "application/zip");
        }
    }
}
