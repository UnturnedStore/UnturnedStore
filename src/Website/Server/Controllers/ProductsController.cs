using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Server.Services;
using Website.Shared.Constants;
using Website.Shared.Enums;
using Website.Shared.Extensions;
using Website.Shared.Models;
using Website.Shared.Models.Database;
using Website.Shared.Params;
using Website.Shared.Results;

namespace Website.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsRepository productsRepository;
        private readonly OffersRepository offersRepository;
        private readonly DiscordService discordService;

        public ProductsController(ProductsRepository productsRepository, OffersRepository offersRepository, DiscordService discordService)
        {
            this.productsRepository = productsRepository;
            this.offersRepository = offersRepository;
            this.discordService = discordService;
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("status")]
        public async Task<IActionResult> PostProductStatusAsync([FromBody] ChangeProductStatusParams parameters)
        {
            PrivateProduct product = await productsRepository.GetPrivateProductAsync(parameters.ProductId);
            if (!User.IsInRole(RoleConstants.AdminRoleId) && product.Seller.Id != User.Id())
            {
                return Unauthorized();
            }

            if (product.Status == ProductStatus.Released && parameters.Status != ProductStatus.Disabled)
            {
                return BadRequest();
            }

            if (parameters.Status == ProductStatus.WaitingForApproval)
            {
                if (((product.Status != ProductStatus.New && product.Status != ProductStatus.Rejected) || product.Price == 0 || product.Seller.IsVerifiedSeller) && product.Status != ProductStatus.Disabled)
                {
                    return BadRequest();
                }

                discordService.SendApproveRequestNotification(product);
            }

            if (parameters.Status == ProductStatus.Rejected || parameters.Status == ProductStatus.Approved)
            {
                if (product.Status != ProductStatus.WaitingForApproval || !User.IsInRole(RoleConstants.AdminRoleId))
                {
                    return BadRequest();
                }
                parameters.AdminId = User.Id();
            }

            if (parameters.Status == ProductStatus.Released)
            {
                if (product.Price > 0 && !product.Seller.IsVerifiedSeller && product.Status != ProductStatus.Approved)
                {
                    return BadRequest();
                }

                await productsRepository.SetProductEnabledAsync(product.Id, true);
                await productsRepository.SetProductReleaseDateAsync(product.Id, DateTime.Now);
                discordService.SendProductRelease(product);
            }

            if (parameters.Status == ProductStatus.Disabled)
            {
                if (product.Status != ProductStatus.Released || !User.IsInRole(RoleConstants.AdminRoleId))
                {
                    return BadRequest();
                }
                parameters.AdminId = User.Id();
                await productsRepository.SetProductEnabledAsync(product.Id, false);
                await offersRepository.CancelProductSales(product.Id);
            }

            await productsRepository.UpdateStatusAsync(parameters);
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

            if (!product.IsEnabled && product.SellerId != userId && product.Customer == null && !User.IsInRole(RoleConstants.AdminRoleId))
            {
                return BadRequest();
            }
            else
            {
                return Ok(product);
            }
        }

        [HttpGet("{productId}/image")]
        public async Task<IActionResult> GetProductImageAsync(int productId)
        {
            int imageId = await productsRepository.GetProductImageIdAsync(productId);
            return Redirect($"/api/images/{imageId}");
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetProductTagsAsync()
        {
            return Ok(await productsRepository.GetTagsAsync());
        }

        [Authorize(Roles = RoleConstants.AdminRoleId)]
        [HttpPost("tags")]
        public async Task<IActionResult> PostProductTagAsync([FromBody] MProductTag tag)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            try
            {
                return Ok(await productsRepository.AddTagAsync(tag));
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    return StatusCode(StatusCodes.Status409Conflict);
                }

                throw e;
            }
        }

        [Authorize(Roles = RoleConstants.AdminRoleId)]
        [HttpPut("tags")]
        public async Task<IActionResult> PutProductTagAsync([FromBody] MProductTag tag)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.UpdateTagAsync(tag);

            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminRoleId)]
        [HttpDelete("tags/{tagid}")]
        public async Task<IActionResult> DeleteProductTagAsync(int tagid)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.DeleteTagAsync(tagid);

            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost]
        public async Task<IActionResult> PostProductAsync([FromBody] MProduct product)
        {
            if (product.Price == 0 && product.IsLoaderEnabled)
            {
                return BadRequest();
            }

            product.SellerId = int.Parse(User.Identity.Name);
            product.Status = ProductStatus.New;

            try
            {
                return Ok(await productsRepository.AddProductAsync(product));
            }
            catch (SqlException e)
            {
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
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if ((product.IsLoaderEnabled && product.Price == 0) || !await offersRepository.CanUpdateProductWithSale(product.Id, product.Price, product.IsEnabled))
            {
                return BadRequest();
            }

            try
            {
                await productsRepository.UpdateProductAsync(product);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    return StatusCode(StatusCodes.Status409Conflict);
                }

                throw e;
            }
            
            await offersRepository.UpdateProductSaleProductPriceAsync(product.Id);
            
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("tabs")]
        public async Task<IActionResult> PostProductTabAsync([FromBody] MProductTab tab)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductSellerAsync(tab.ProductId, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            return Ok(await productsRepository.AddProductTabAsync(tab));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPut("tabs")]
        public async Task<IActionResult> PutProductTabAsync([FromBody] MProductTab tab)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductTabSellerAsync(tab.Id, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.UpdateProductTabAsync(tab);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("tabs/{tabId}")]
        public async Task<IActionResult> DeleteProductTabAsync(int tabId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductTabSellerAsync(tabId, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.DeleteProductTabAsync(tabId);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("medias")]
        public async Task<IActionResult> PostProductMediaAsync([FromBody] MProductMedia media)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductSellerAsync(media.ProductId, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            return Ok(await productsRepository.AddProductMediaAsync(media));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("medias/{mediaId}")]
        public async Task<IActionResult> DeleteProductMediaAsync(int mediaId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductMediaSellerAsync(mediaId, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.DeleteProductMediaAsync(mediaId);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("customers")]
        public async Task<IActionResult> PostProductCustomerAsync([FromBody] MProductCustomer customer)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductSellerAsync(customer.ProductId, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            return Ok(await productsRepository.AddProductCustomerAsync(customer));
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPut("customers")]
        public async Task<IActionResult> PutProductCustomerAsync([FromBody] MProductCustomer customer)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductCustomerSellerAsync(customer.Id, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.UpdateProductCustomerAsync(customer);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("customers/{customerId}")]
        public async Task<IActionResult> DeleteProductCustomerAsync(int customerId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductCustomerSellerAsync(customerId, int.Parse(User.Identity.Name)))
            {
                return Unauthorized();
            }

            await productsRepository.DeleteProductCustomerAsync(customerId);
            return Ok();
        }

        [Authorize]
        [HttpPost("reviews")]
        public async Task<IActionResult> PostProductReviewAsync([FromBody] MProductReview review)
        {
            review.UserId = int.Parse(User.Identity.Name);
            if (!await productsRepository.CanReviewProductAsync(review.ProductId, review.UserId))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            review = await productsRepository.AddProductReviewAsync(review);
            await discordService.SendReviewAsync(review);
            return Ok(review);
        }

        [Authorize]
        [HttpPut("reviews")]
        public async Task<IActionResult> PutProductReviewAsync([FromBody] MProductReview review)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductReviewOwnerAsync(review.Id, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.UpdateProductReviewAsync(review);
            return Ok();
        }

        [Authorize]
        [HttpDelete("reviews/{reviewId}")]
        public async Task<IActionResult> DeleteProductReviewAsync(int reviewId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductReviewOwnerAsync(reviewId, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.DeleteProductReviewAsync(reviewId);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("reviews/replies")]
        public async Task<IActionResult> PostProductReviewReplyAsync([FromBody] MProductReviewReply reply)
        {
            reply.UserId = int.Parse(User.Identity.Name);
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.CanReplyReviewAsync(reply.ReviewId, reply.UserId))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            reply = await productsRepository.AddProductReviewReplyAsync(reply);
            return Ok(reply);
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPut("reviews/replies")]
        public async Task<IActionResult> PutProductReviewReplyAsync([FromBody] MProductReviewReply reply)
        {
            if (!await productsRepository.IsProductReviewReplyOwnerAsync(reply.Id, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.UpdateProductReviewReplyAsync(reply);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("reviews/replies/{replyId}")]
        public async Task<IActionResult> DeleteProductReviewReplyAsync(int replyId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductReviewReplyOwnerAsync(replyId, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.DeleteProductReviewReplyAsync(replyId);
            return Ok();
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPost("workshop")]
        public async Task<IActionResult> PostProductWorkshopItemAsync([FromBody] MProductWorkshopItem workshopItem)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductSellerAsync(workshopItem.ProductId, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            try
            {
                return Ok(await productsRepository.AddProductWorkshopItemAsync(workshopItem));
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    return StatusCode(StatusCodes.Status409Conflict);
                }

                throw e;
            }
        }

        [HttpPost("workshop/verify")]
        public async Task<IActionResult> VerifyProductWorkshopItemAsync([FromBody] List<MProductWorkshopItem> workshopItems)
        {
            if (workshopItems == null || workshopItems.Count == 0 || workshopItems.Count == 1 && workshopItems[0].WorkshopFileId == 0)
            {
                return NoContent();
            }

            if (workshopItems.Count > 1 && !workshopItems.Skip(1).All(w => w.ProductId == workshopItems[0].ProductId))
            {
                return BadRequest();
            }

            WorkshopItemResult workshopResult;
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(WorkshopItemResult.WorkshopItemsUrl(), WorkshopItemResult.BuildWorkshopItemsFormData(workshopItems.Select(w => w.WorkshopFileId).ToArray()));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    workshopResult = await response.Content.ReadFromJsonAsync<WorkshopItemResult>();
                }
                else
                {
                    return StatusCode((int)response.StatusCode);
                }
            }

            return Ok(workshopResult);
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpPut("workshop")]
        public async Task<IActionResult> PutProductWorkshopItemAsync([FromBody] MProductWorkshopItem workshopItem)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductWorkshopItemSellerAsync(workshopItem.Id, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            try
            {
                await productsRepository.UpdateProductWorkshopItemAsync(workshopItem);
                return Ok();
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    return StatusCode(StatusCodes.Status409Conflict);
                }

                throw e;
            }
        }

        [Authorize(Roles = RoleConstants.AdminAndSeller)]
        [HttpDelete("workshop/{workshopId}")]
        public async Task<IActionResult> DeleteProductWorkshopItemAsync(int workshopId)
        {
            if (!User.IsInRole(RoleConstants.AdminRoleId) && !await productsRepository.IsProductWorkshopItemSellerAsync(workshopId, int.Parse(User.Identity.Name)))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            await productsRepository.DeleteProductWorkshopItemAsync(workshopId);
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
