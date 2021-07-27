using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Models;

namespace Website.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ImagesRepository imagesRepository;

        public ImagesController(ImagesRepository imagesRepository)
        {
            this.imagesRepository = imagesRepository;
        }

        [HttpGet("{imageId}")]
        public async Task<IActionResult> GetImageAsync(int imageId)
        {
            var img = await imagesRepository.GetImageAsync(imageId);
            Response.Headers.Add("Content-Disposition", "inline; filename=" + img.Name);
            return File(img.Content, img.ContentType);
        }

        [HttpPost]
        public async Task<IActionResult> PostImageAsync([FromBody] MImage image)
        {
            return Ok(await imagesRepository.AddImageAsync(image));
        }
    }
}
