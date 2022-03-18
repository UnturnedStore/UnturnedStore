using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Models.Database;

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
            if (img == null)
            {
                return NotFound();
            }

            Response.Headers.Add("Content-Disposition", "inline; filename=" + img.Name);
            return File(img.Content, img.ContentType);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostImageAsync([FromBody] MImage image)
        {
            image.UserId = int.Parse(User.Identity.Name);
            return Ok(await imagesRepository.AddImageAsync(image));
        }
    }
}
