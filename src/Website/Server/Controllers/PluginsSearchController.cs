using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Website.Server.Services;

namespace Website.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PluginsSearchController : ControllerBase
{
    [HttpGet("query")]
    public async Task<ActionResult> Query(PluginsSearchService pluginsSearchService, 
        IMemoryCache cache, [FromQuery] string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return BadRequest();
        }

        var key = $"{nameof(PluginsSearchController)}_{nameof(Query)}:{hash}";
        if (cache.TryGetValue(key, out IReadOnlyCollection<PluginSearchDto> result))
        {
            return Ok(result);
        }

        result = await pluginsSearchService.SearchByHash(hash);
        if (result.Count > 0)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                               .SetAbsoluteExpiration(TimeSpan.FromDays(7))
                               .SetSlidingExpiration(TimeSpan.FromHours(24));
            cache.Set(key, result, cacheOptions);
        }
        
        return Ok(result);
    }
}