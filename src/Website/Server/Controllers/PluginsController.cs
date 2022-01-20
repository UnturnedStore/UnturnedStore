using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Models.Database;
using Website.Shared.Params;
using Website.Shared.Results;

namespace Website.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PluginsController : ControllerBase
    {
        private readonly PluginsRepository loaderRepository;

        public PluginsController(PluginsRepository loaderRepository)
        {
            this.loaderRepository = loaderRepository;
        }

        [HttpPost]
        public async Task<IActionResult> PostPluginAsync([FromBody] GetPluginParams @params)
        {
            GetPluginResult result = await loaderRepository.GetPluginAsync(@params);

            if (result.ReturnCode != 0)
            {
                return BadRequest(result);
            }

            return File(result.Version.Content, result.Version.ContentType, result.Version.FileName);
        }
    }
}
