using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Data.Repositories
{
    public class OffersRepository
    {
        private readonly SqlConnection connection;

        public OffersRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<bool> IsProductSaleExpiredAsync(int productSaleId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.ProductSales WHERE Id = @productSaleId AND IsExpired = 1;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productSaleId });
        }

        public async Task<bool> IsProductSaleSellerAsync(int productSaleId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.ProductSales s JOIN dbo.Products p ON p.Id = s.ProductId " +
                "WHERE s.Id = @productSaleId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productSaleId, userId });
        }

        public async Task<bool> CanProductHaveOfferAsync(int productId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Products p WHERE p.Id = @productId AND p.IsEnabled = 1 AND p.Price > 0 AND p.Status = 4;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productId });
        }

        public async Task<bool> CanUpdateProductSaleAsync (int productSaleId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.ProductSales WHERE Id = @productSaleId AND IsExpired = 0 AND IsActive = 0;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productSaleId });
        }

        public async Task<bool> CanUpdateProductWithSale(int productId, decimal newPrice, bool newEnabled)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Products p LEFT JOIN dbo.ProductSales ps ON ps.ProductId = p.Id AND ps.IsActive = 1 AND ps.IsExpired = 0 WHERE p.Id = @productId AND (ps.Id IS NULL OR (ps.IsActive = 1 AND p.Price = @newPrice AND @newEnabled = 1));";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productId, newPrice, newEnabled });
        }

        public async Task<IEnumerable<MProductSale>> GetSellerProductSalesAsync(int userId)
        {
            const string sql = @"SELECT ps.*, (SELECT COUNT(*) FROM dbo.OrderItems oi WHERE oi.SaleId = ps.Id) AS SaleUsageCount 
                FROM dbo.ProductSales ps
                WHERE EXISTS (SELECT * FROM dbo.Products p WHERE p.Id = ps.ProductId AND p.SellerId = @userId)";

            return await connection.QueryAsync<MProductSale>(sql, new { userId });
        }

        public async Task<DateTime> GetLastProductSaleEndDateAsync(int productSaleId, int productId)
        {
            const string sql = "SELECT TOP 1 EndDate FROM dbo.ProductSales WHERE Id != @productSaleId AND ProductId = @productId ORDER BY StartDate DESC;";
            return await connection.QuerySingleOrDefaultAsync<DateTime>(sql, new { productSaleId, productId });
        }

        public async Task<MProductSale> AddProductSaleAsync(MProductSale productSale)
        {
            const string sql = "INSERT INTO dbo.ProductSales (ProductId, ProductPrice, SaleName, SaleMultiplier, StartDate, EndDate) " +
            "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.ProductPrice, INSERTED.SaleName, INSERTED.SaleMultiplier, INSERTED.StartDate, INSERTED.EndDate " +
            "VALUES (@ProductId, (SELECT Price FROM dbo.Products p WHERE p.Id = @ProductId), @SaleName, @SaleMultiplier, @StartDate, @EndDate);";

            if (productSale.StartDate == null) productSale.StartDate = DateTime.Now;
            return await connection.QuerySingleAsync<MProductSale>(sql, productSale);
        }

        public async Task UpdateProductSaleAsync(MProductSale productSale)
        {
            const string sql = "UPDATE dbo.ProductSales SET ProductId = @ProductId, SaleName = @SaleName, SaleMultiplier = @SaleMultiplier, StartDate = @StartDate, EndDate = @EndDate WHERE Id = @Id;";

            if (productSale.StartDate == null) productSale.StartDate = DateTime.Now;
            await connection.ExecuteAsync(sql, productSale);
        }
        
        public async Task UpdateProductSaleProductPriceAsync(int productId)
        {
            const string sql = "UPDATE dbo.ProductSales SET ProductPrice = (SELECT Price FROM dbo.Products p WHERE p.Id = @productId) WHERE ProductId = @productId AND IsActive = 0 AND IsExpired = 0;";
            await connection.ExecuteAsync(sql, new { productId });
        }
        
        public async Task EndProductSaleAsync(int productSaleId)
        {
            const string sql = "UPDATE dbo.ProductSales SET EndDate = SYSDATETIME() WHERE Id = @productSaleId;";
            await connection.ExecuteAsync(sql, new { productSaleId });
        }

        public async Task DeleteProductSaleAsync(int productSaleId)
        {
            const string sql = "DELETE FROM dbo.ProductSales WHERE Id = @productSaleId;";
            await connection.ExecuteAsync(sql, new { productSaleId });
        }

        public async Task CancelProductSales(int productId)
        {
            const string sql2 = "UPDATE dbo.ProductSales SET EndDate = SYSDATETIME() WHERE ProductId = @productId AND IsActive = 1 AND IsExpired = 0;";
            await connection.ExecuteAsync(sql2, new { productId });
        }

        public async Task<bool> IsProductCouponSellerAsync(int productCouponId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.ProductCoupons co JOIN dbo.Products p ON p.Id = co.ProductId " +
            "WHERE co.Id = @productCouponId AND co.IsDeleted = 0 AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productCouponId, userId });
        }

        public async Task<bool> GetCouponFromCodeAsync(string couponCode)
        {
            const string sql = "SELECT co.Id, co.ProductId, co.CouponName, co.CouponCode, co.CouponMultiplier FROM dbo.ProductCoupons co " +
            "WHERE IsDeleted = 0 AND co.IsEnabled = 1 AND co.CouponCode = @couponCode AND (co.MaxUses = 0 OR (SELECT COUNT(o.Id) FROM dbo.OrderItems o WHERE o.CouponId = co.Id) < co.MaxUses);";
            return await connection.ExecuteScalarAsync<bool>(sql, new { couponCode });
        }

        public async Task<MProductCoupon> GetCouponFromCodeAsync(string couponCode, int productId)
        {
            const string sql = "SELECT co.Id, co.ProductId, co.CouponName, co.CouponCode, co.CouponMultiplier FROM dbo.ProductCoupons co " +
            "WHERE IsDeleted = 0 AND co.IsEnabled = 1 AND co.ProductId = @productId AND co.CouponCode = @couponCode AND (co.MaxUses = 0 OR (SELECT COUNT(o.Id) FROM dbo.OrderItems o WHERE o.CouponId = co.Id) < co.MaxUses);";
            return await connection.QuerySingleOrDefaultAsync<MProductCoupon>(sql, new { couponCode, productId });
        }

        public async Task<MProductCoupon> GetCouponFromCodeAsync(string couponCode, List<OrderItemParams> orderItems)
        {
            const string sql0 = @"SELECT co.Id, co.ProductId, co.CouponName, co.CouponCode, co.CouponMultiplier, co.IsEnabled 
                FROM dbo.ProductCoupons co 
                WHERE IsDeleted = 0 AND co.IsEnabled = 1 AND co.ProductId IN (";
            const string sql1 = ") AND co.CouponCode = @couponCode AND (co.MaxUses = 0 OR (SELECT COUNT(o.Id) FROM dbo.OrderItems o WHERE o.CouponId = co.Id) < co.MaxUses);";
            return await connection.QuerySingleOrDefaultAsync<MProductCoupon>(sql0 + string.Join(", ", orderItems.Select(i => i.ProductId)) + sql1, new { couponCode });
        }

        public async Task<IEnumerable<MProductCoupon>> GetSellerProductCouponsAsync(int userId)
        {
            const string sql = @"SELECT pc.*, (SELECT COUNT(*) FROM dbo.OrderItems oi WHERE oi.CouponId = pc.Id) AS CouponUsageCount 
                FROM dbo.ProductCoupons pc
                WHERE EXISTS(SELECT* FROM dbo.Products p WHERE p.Id = pc.ProductId AND p.SellerId = @userId);";

            return await connection.QueryAsync<MProductCoupon>(sql, new { userId });
        }

        public async Task<MProductCoupon> AddProductCouponAsync(MProductCoupon productCoupon)
        {
            const string sql = "INSERT INTO dbo.ProductCoupons (ProductId, CouponName, CouponCode, CouponMultiplier, MaxUses, IsEnabled) " +
            "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.CouponName, INSERTED.CouponCode, INSERTED.CouponMultiplier, INSERTED.MaxUses, INSERTED.IsEnabled " +
            "VALUES (@ProductId, @CouponName, @CouponCode, @CouponMultiplier, @MaxUses, @IsEnabled);";
            return await connection.QuerySingleAsync<MProductCoupon>(sql, productCoupon);
        }

        public async Task UpdateProductCouponAsync(MProductCoupon productCoupon)
        {
            const string sql = "UPDATE dbo.ProductCoupons SET ProductId = @ProductId, CouponName = @CouponName, CouponCode = @CouponCode, CouponMultiplier = @CouponMultiplier, MaxUses = @MaxUses, IsEnabled = @IsEnabled WHERE Id = @Id AND IsDeleted = 0;";
            await connection.ExecuteAsync(sql, productCoupon);
        }

        public async Task DeleteProductCouponAsync(int productCouponId)
        {
            const string sql0 = "SELECT COUNT(*) FROM dbo.OrderItems WHERE CouponId = @productCouponId;";
            int CouponUsageCount = await connection.ExecuteScalarAsync<int>(sql0, new { productCouponId });

            if (CouponUsageCount == 0)
            {
                const string sql1 = "DELETE FROM dbo.ProductCoupons WHERE Id = @productCouponId;";
                await connection.ExecuteAsync(sql1, new { productCouponId });
            }
            else
            {
                const string sql2 = "UPDATE dbo.ProductCoupons SET IsDeleted = 1 WHERE Id = @productCouponId;";
                await connection.ExecuteAsync(sql2, new { productCouponId });
            }
        }
    }
}
