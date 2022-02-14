using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Server.Services;
using Website.Shared.Constants;
using Website.Shared.Enums;
using Website.Shared.Extensions;
using Website.Shared.Models;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsRepository productsRepository;
        private readonly DiscordService discordService;

        public ProductsController(ProductsRepository productsRepository, DiscordService discordService)
        {
            this.productsRepository = productsRepository;
            this.discordService = discordService;
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("status")]
        public async Task<IActionResult> PostProductStatusAsync([FromBody] ChangeProductStatusParams @params)
        {
            PrivateProduct product = await productsRepository.GetPrivateProductAsync(@params.ProductId);

            if (!User.IsInRole(RoleConstants.AdminRoleId) && product.Seller.Id != User.Id())
            {
                return Unauthorized();
            }

            if (product.Status == ProductStatus.Released)
            {
                return BadRequest();
            }

            if (@params.Status == ProductStatus.WaitingForApproval)
            {
                if ((product.Status != ProductStatus.New && product.Status != ProductStatus.Rejected) || product.Price == 0)
                {
                    return BadRequest();
                }
            }

            if (@params.Status == ProductStatus.Rejected || @params.Status == ProductStatus.Approved)
            {
                if (product.Status != ProductStatus.WaitingForApproval || !User.IsInRole(RoleConstants.AdminRoleId))
                {
                    return BadRequest();
                }
                @params.AdminId = User.Id();
            }

            if (@params.Status == ProductStatus.Released)
            {
                if (product.Price > 0 && !product.Seller.IsVerifiedSeller && product.Status != ProductStatus.Approved)
                {
                    return BadRequest();
                }

                await productsRepository.SetProductEnabledAsync(product.Id, true);
                discordService.SendProductRelease(product);
            }

            await productsRepository.UpdateStatusAsync(@params);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsAsync()
        {
            int userId = User.Identity?.IsAuthenticated ?? false ? int.Parse(User.Identity.Name) : 0;
            var products = await productsRepository.GetProductsAsync(userId);
                
            return Ok(products);
        }

        [HttpGet("user/{sellerId}")]
        public async Task<IActionResult> GetUserProductsAsync(int sellerId)
        {
            var products = await productsRepository.GetUserProductsAsync(sellerId);
            int userId = User.Identity?.IsAuthenticated ?? false ? int.Parse(User.Identity.Name) : 0;

            return Ok(products.Where(x => x.IsEnabled || x.SellerId == userId));
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductAsync(int productId)
        {
            int userId = User.Identity?.IsAuthenticated ?? false ? int.Parse(User.Identity.Name) : 0;
            var product = await productsRepository.GetProductAsync(productId, userId);
            
            if (product == null)
            {
                return NoContent();
            }

            if (!product.IsEnabled && product.SellerId != userId && product.Customer == null)
                return BadRequest();
            else
                return Ok(product);
        }

        [HttpGet("{productId}/image")]
        public async Task<IActionResult> GetProductImageAsync(int productId)
        {
            int imageId = await productsRepository.GetProductImageIdAsync(productId);
            return Redirect($"/api/images/{imageId}");
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost]
        public async Task<IActionResult> PostProductAsync([FromBody] MProduct product)
        {
            // Don't let free products use loader
            if (product.Price == 0 && product.IsLoaderEnabled)
            {
                return BadRequest();
            }

            product.SellerId = int.Parse(User.Identity.Name);
            product.Status = ProductStatus.New;

            try
            {
                return Ok(await productsRepository.AddProductAsync(product));
            } catch (SqlException e)
            {
                // Duplicate key exception number
                if (e.Number == 2627)
                {
                    return StatusCode(StatusCodes.Status409Conflict);
                }

                throw e;
            }
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPut]
        public async Task<IActionResult> PutProductAsync([FromBody] MProduct product)
        {            
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductSellerAsync(product.Id, int.Parse(User.Identity.Name)))
                return StatusCode(StatusCodes.Status401Unauthorized);

            if (product.IsLoaderEnabled && product.Price == 0) 
            {
                return BadRequest();
            }

            try
            {
                await productsRepository.UpdateProductAsync(product);
            }
            catch (SqlException e)
            {
                // Duplicate key exception number
                if (e.Number == 2627)
                {
                    return StatusCode(StatusCodes.Status409Conflict);
                }

                throw e;
            }
            
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("tabs")]
        public async Task<IActionResult> PostProductTabAsync([FromBody] MProductTab tab)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductSellerAsync(tab.ProductId, int.Parse(User.Identity.Name)))
                return StatusCode(StatusCodes.Status401Unauthorized);

            return Ok(await productsRepository.AddProductTabAsync(tab));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPut("tabs")]
        public async Task<IActionResult> PutProductTabAsync([FromBody] MProductTab tab)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductTabSellerAsync(tab.Id, int.Parse(User.Identity.Name)))
                return StatusCode(StatusCodes.Status401Unauthorized);

            await productsRepository.UpdateProductTabAsync(tab);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("tabs/{tabId}")]
        public async Task<IActionResult> DeleteProductTabAsync(int tabId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductTabSellerAsync(tabId, int.Parse(User.Identity.Name)))
                return StatusCode(StatusCodes.Status401Unauthorized);

            await productsRepository.DeleteProductTabAsync(tabId);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("medias")]
        public async Task<IActionResult> PostProductMediaAsync([FromBody] MProductMedia media)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductSellerAsync(media.ProductId, int.Parse(User.Identity.Name)))
                return StatusCode(StatusCodes.Status401Unauthorized);

            return Ok(await productsRepository.AddProductMediaAsync(media));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("medias/{mediaId}")]
        public async Task<IActionResult> DeleteProductMediaAsync(int mediaId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductMediaSellerAsync(mediaId, int.Parse(User.Identity.Name)))
                return StatusCode(StatusCodes.Status401Unauthorized);

            await productsRepository.DeleteProductMediaAsync(mediaId);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("customers")]
        public async Task<IActionResult> PostProductCustomerAsync([FromBody] MProductCustomer customer)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductSellerAsync(customer.ProductId, int.Parse(User.Identity.Name)))
                return StatusCode(StatusCodes.Status401Unauthorized);

            return Ok(await productsRepository.AddProductCustomerAsync(customer));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("customers/{customerId}")]
        public async Task<IActionResult> DeleteProductCustomerAsync(int customerId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductCustomerSellerAsync(customerId, int.Parse(User.Identity.Name)))
                return StatusCode(StatusCodes.Status401Unauthorized);

            await productsRepository.DeleteProductCustomerAsync(customerId);
            return Ok();
        }
        
        [Authorize]
        [HttpPost("reviews")]
        public async Task<IActionResult> PostProductReviewAsync([FromBody] MProductReview review)
        {
            review.UserId = int.Parse(User.Identity.Name);
            if (!await productsRepository.CanReviewProductAsync(review.ProductId, review.UserId))
                return StatusCode(StatusCodes.Status401Unauthorized);

            review = await productsRepository.AddProductReviewAsync(review);
            await discordService.SendReviewAsync(review, Request.Headers["Origin"]);
            return Ok(review);
        }

        [Authorize]
        [HttpPut("reviews")]
        public async Task<IActionResult> PutProductReviewAsync([FromBody] MProductReview review)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductReviewOwnerAsync(review.Id, int.Parse(User.Identity.Name)))
                return StatusCode(StatusCodes.Status401Unauthorized);

            await productsRepository.UpdateProductReviewAsync(review);
            return Ok();
        }

        [Authorize]
        [HttpDelete("reviews/{reviewId}")]
        public async Task<IActionResult> DeleteProductReviewAsync(int reviewId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductReviewOwnerAsync(reviewId, int.Parse(User.Identity.Name)))
                return StatusCode(StatusCodes.Status401Unauthorized);

            await productsRepository.DeleteProductReviewAsync(reviewId);
            return Ok();
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetUserProductsAsync()
        {
            return Ok(await productsRepository.GetMyProductsAsync(int.Parse(User.Identity.Name)));
        }
    }
}
