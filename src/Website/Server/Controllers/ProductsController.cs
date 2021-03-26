using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsRepository productsRepository;

        public ProductsController(ProductsRepository productsRepository)
        {
            this.productsRepository = productsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsAsync()
        {
            return Ok(await productsRepository.GetProductsAsync());
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductAsync(int productId)
        {
            int userId = User.Identity?.IsAuthenticated ?? false ? int.Parse(User.Identity.Name) : 0;
            return Ok(await productsRepository.GetProductAsync(productId, userId));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost]
        public async Task<IActionResult> PostProductAsync([FromBody] ProductModel product)
        {
            product.SellerId = int.Parse(User.Identity.Name);
            return Ok(await productsRepository.AddProductAsync(product));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPut]
        public async Task<IActionResult> PutProductAsync([FromBody] ProductModel product)
        {            
            if (!await productsRepository.IsProductSellerAsync(product.Id, int.Parse(User.Identity.Name)))
                return BadRequest();

            await productsRepository.UpdateProductAsync(product);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("tabs")]
        public async Task<IActionResult> PostProductTabAsync([FromBody] ProductTabModel tab)
        {
            if (!await productsRepository.IsProductSellerAsync(tab.ProductId, int.Parse(User.Identity.Name)))
                return BadRequest();

            return Ok(await productsRepository.AddProductTabAsync(tab));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPut("tabs")]
        public async Task<IActionResult> PutProductTabAsync([FromBody] ProductTabModel tab)
        {
            if (!await productsRepository.IsProductTabSellerAsync(tab.Id, int.Parse(User.Identity.Name)))
                return BadRequest();

            await productsRepository.UpdateProductTabAsync(tab);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("tabs/{tabId}")]
        public async Task<IActionResult> DeleteProductTabAsync(int tabId)
        {
            if (!await productsRepository.IsProductTabSellerAsync(tabId, int.Parse(User.Identity.Name)))
                return BadRequest();

            await productsRepository.DeleteProductTabAsync(tabId);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("medias")]
        public async Task<IActionResult> PostProductMediaAsync([FromBody] ProductMediaModel media)
        {
            if (!await productsRepository.IsProductSellerAsync(media.ProductId, int.Parse(User.Identity.Name)))
                return BadRequest();

            return Ok(await productsRepository.AddProductMediaAsync(media));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("medias/{mediaId}")]
        public async Task<IActionResult> DeleteProductMediaAsync(int mediaId)
        {
            if (!await productsRepository.IsProductMediaSellerAsync(mediaId, int.Parse(User.Identity.Name)))
                return BadRequest();

            await productsRepository.DeleteProductMediaAsync(mediaId);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("customers")]
        public async Task<IActionResult> PostProductCustomerAsync([FromBody] ProductCustomerModel customer)
        {
            if (!await productsRepository.IsProductSellerAsync(customer.ProductId, int.Parse(User.Identity.Name)))
                return BadRequest();

            return Ok(await productsRepository.AddProductCustomerAsync(customer));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("customers/{customerId}")]
        public async Task<IActionResult> DeleteProductCustomerAsync(int customerId)
        {
            if (!await productsRepository.IsProductCustomerSellerAsync(customerId, int.Parse(User.Identity.Name)))
                return BadRequest();

            await productsRepository.DeleteProductCustomerAsync(customerId);
            return Ok();
        }
        
        [Authorize]
        [HttpPost("reviews")]
        public async Task<IActionResult> PostProductReviewAsync([FromBody] ProductReviewModel review)
        {
            review.UserId = int.Parse(User.Identity.Name);
            if (!await productsRepository.CanReviewProductAsync(review.ProductId, review.UserId))
                return BadRequest();

            return Ok(await productsRepository.AddProductReviewAsync(review));
        }

        [Authorize]
        [HttpPut("reviews")]
        public async Task<IActionResult> PutProductReviewAsync([FromBody] ProductReviewModel review)
        {
            if (!await productsRepository.IsProductReviewOwnerAsync(review.Id, int.Parse(User.Identity.Name)))
                return BadRequest();

            await productsRepository.UpdateProductReviewAsync(review);
            return Ok();
        }

        [Authorize]
        [HttpDelete("reviews/{reviewId}")]
        public async Task<IActionResult> DeleteProductReviewAsync(int reviewId)
        {
            if (!await productsRepository.IsProductReviewOwnerAsync(reviewId, int.Parse(User.Identity.Name)))
                return BadRequest();

            await productsRepository.DeleteProductReviewAsync(reviewId);
            return Ok();
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetUserProductsAsync()
        {
            return Ok(await productsRepository.GetUserProductsAsync(int.Parse(User.Identity.Name)));
        }
    }
}
