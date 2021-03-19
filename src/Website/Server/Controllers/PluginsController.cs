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
        private readonly PluginsRepository pluginsRepository;
        private readonly BranchesRepository branchesRepository;
        private readonly PluginService pluginService;
        private readonly DiscordService discordService;

        public PluginsController(PluginsRepository pluginsRepository, BranchesRepository branchesRepository, PluginService pluginService, 
            DiscordService discordService)
        {
            this.pluginsRepository = pluginsRepository;
            this.branchesRepository = branchesRepository;
            this.pluginService = pluginService;
            this.discordService = discordService;
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost]
        public async Task<IActionResult> PostPluginAsync([FromBody] PluginModel plugin)
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
            if (!await pluginsRepository.IsPluginOwnerAsync(pluginId, int.Parse(User.Identity.Name)))
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

            bool isOwner = await pluginsRepository.IsPluginOwnerAsync(pluginId, userId);
            var plugin = await pluginsRepository.GetPluginAsync(pluginId, isOwner);
            if (!isOwner && plugin.Branch.Product.Price > 0)
            {
                if (!await pluginsRepository.IsPluginCustomerAsync(pluginId, userId))
                    return Unauthorized();
            }

            await pluginsRepository.IncrementDownloadsCount(pluginId);

            Response.Headers.Add("Content-Disposition", "inline; filename=" + 
                string.Concat(plugin.Branch.Product.Name, "-", plugin.Branch.Name, "-", plugin.Version, ".zip"));
            return File(pluginService.ZipPlugin(plugin), "application/zip");
        }
    }
}
