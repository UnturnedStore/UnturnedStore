using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
            MImage image = await imagesRepository.GetImageAsync(imageId);
            if (image == null)
            {
                return NotFound();
            }

            Response.Headers.Add("Content-Disposition", "inline; filename=" + image.Name);
            return File(image.Content, image.ContentType);
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
