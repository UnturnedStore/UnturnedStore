using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Server.Controllers
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    [Route("api/[controller]")]
    [ApiController]
    public class OffersController : ControllerBase
    {
        private readonly OffersRepository offersRepository;
        private readonly ProductsRepository productsRepository;

        public OffersController(OffersRepository offersRepository, ProductsRepository productsRepository)
        {
            this.offersRepository = offersRepository;
            this.productsRepository = productsRepository;
        }

        [HttpGet("sales")]
        public async Task<IActionResult> GetProductSales()
        {
            return Ok(await offersRepository.GetSellerProductSalesAsync(int.Parse(User.Identity.Name)));
        }

        [HttpPost("sales")]
        public async Task<IActionResult> AddProductSale([FromBody] MProductSale productSale)
        {
            if (!await productsRepository.IsProductSellerAsync(productSale.ProductId, int.Parse(User.Identity.Name)))
            {
                return Unauthorized();
            }

            if (productSale.SaleMultiplier <= 0 || productSale.SaleMultiplier >= 1 || !await offersRepository.CanProductHaveOfferAsync(productSale.ProductId) || !await ValidDurationDates(productSale))
            {
                return BadRequest();
            }

            return Ok(await offersRepository.AddProductSaleAsync(productSale));
        }

        private async Task<bool> ValidDurationDates(MProductSale productSale)
        {
            return (productSale.StartDate > DateTime.Now || productSale.StartDate == null) && (productSale.EndDate > productSale.SaleStart) && (productSale.EndDate - productSale.SaleStart) < TimeSpan.FromDays(ProductSalesConstants.SaleMaxDurationDays) && (await offersRepository.GetLastProductSaleEndDateAsync(productSale.Id, productSale.ProductId)).AddDays(ProductSalesConstants.SaleCooldownDays) < productSale.SaleStart;
        }

        [HttpPut("sales")]
        public async Task<IActionResult> PutProductSale([FromBody] MProductSale productSale)
        {
            if (!await productsRepository.IsProductSellerAsync(productSale.ProductId, int.Parse(User.Identity.Name)))
            {
                return Unauthorized();
            }

            if (productSale.SaleMultiplier <= 0 || productSale.SaleMultiplier >= 1 || !await offersRepository.CanUpdateProductSaleAsync(productSale.Id) || !await ValidDurationDates(productSale))
            {
                return BadRequest();
            }

            await offersRepository.UpdateProductSaleAsync(productSale);
            return Ok();
        }

        [HttpDelete("sales/{productSaleId}/expire")]
        public async Task<IActionResult> ExpireProductSale(int productSaleId)
        {
            if (!await offersRepository.IsProductSaleSellerAsync(productSaleId, int.Parse(User.Identity.Name)))
            {
                return Unauthorized();
            }

            if (await offersRepository.CanUpdateProductSaleAsync(productSaleId) || await offersRepository.IsProductSaleExpiredAsync(productSaleId))
            {
                return BadRequest();
            }

            await offersRepository.EndProductSaleAsync(productSaleId);
            return Ok();
        }

        [HttpDelete("sales/{productSaleId}")]
        public async Task<IActionResult> DeleteProductSale(int productSaleId)
        {
            if (!await offersRepository.IsProductSaleSellerAsync(productSaleId, int.Parse(User.Identity.Name)))
            {
                return Unauthorized();
            }

            if (!await offersRepository.CanUpdateProductSaleAsync(productSaleId))
            {
                return BadRequest();
            }

            await offersRepository.DeleteProductSaleAsync(productSaleId);
            return Ok();
        }

        [HttpPost("coupons/{couponCode}")]
        public async Task<IActionResult> GetProductCoupon(string couponCode, [FromBody] List<OrderItemParams> orderItems)
        {
            if (!await offersRepository.GetCouponFromCodeAsync(couponCode))
                return NotFound();

            MProductCoupon coupon = await offersRepository.GetCouponFromCodeAsync(couponCode, orderItems);

            if (coupon == null)
                return BadRequest();

            return Ok(coupon);
        }

        [HttpGet("coupons")]
        public async Task<IActionResult> GetProductCoupons()
        {
            return Ok(await offersRepository.GetSellerProductCouponsAsync(int.Parse(User.Identity.Name)));
        }

        [HttpPost("coupons")]
        public async Task<IActionResult> AddProductCoupon([FromBody] MProductCoupon productCoupon)
        {
            if (!await productsRepository.IsProductSellerAsync(productCoupon.ProductId, int.Parse(User.Identity.Name)))
            {
                return Unauthorized();
            }

            if (productCoupon.CouponMultiplier <= 0 || productCoupon.CouponMultiplier >= 1 || !await offersRepository.CanProductHaveOfferAsync(productCoupon.ProductId))
            {
                return BadRequest();
            }

            try
            {
                return Ok(await offersRepository.AddProductCouponAsync(productCoupon));
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

        [HttpPut("coupons")]
        public async Task<IActionResult> PutProductCoupon([FromBody] MProductCoupon productCoupon)
        {
            if (!await productsRepository.IsProductSellerAsync(productCoupon.ProductId, int.Parse(User.Identity.Name)))
            {
                return Unauthorized();
            }

            if (productCoupon.CouponMultiplier <= 0 || productCoupon.CouponMultiplier >= 1)
            {
                return BadRequest();
            }

            try
            {
                await offersRepository.UpdateProductCouponAsync(productCoupon);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    return StatusCode(StatusCodes.Status409Conflict);
                }

                throw e;
            }

            return Ok();
        }

        [HttpDelete("coupons/{productCouponId}")]
        public async Task<IActionResult> DeleteProductCoupon(int productCouponId)
        {
            if (!await offersRepository.IsProductCouponSellerAsync(productCouponId, int.Parse(User.Identity.Name)))
            {
                return Unauthorized();
            }

            await offersRepository.DeleteProductCouponAsync(productCouponId);
            return Ok();
        }
    }
}
