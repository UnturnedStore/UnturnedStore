using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models;
using Website.Shared.Models.Database;   
using Website.Shared.Params;

namespace Website.Data.Repositories
{
    public class ProductsRepository
    {
        private readonly SqlConnection connection;

        public ProductsRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task SetProductEnabledAsync(int productId, bool isEnabled)
        {
            const string sql = "UPDATE dbo.Products SET IsEnabled = @isEnabled WHERE Id = @productId;";
            await connection.ExecuteAsync(sql, new { productId, isEnabled });
        }

        public async Task<ProductInfo> GetProductInfoAsync(int productId)
        {
            const string sql = "SELECT p.*, u.* FROM dbo.Products p JOIN dbo.Users u ON u.Id = p.SellerId WHERE p.Id = @productId;";
            return (await connection.QueryAsync<ProductInfo, Seller, ProductInfo>(sql, (p, u) =>
            {
                p.Seller = u;
                return p;
            }, new { productId })).FirstOrDefault();
        }

        public async Task<PrivateProduct> GetPrivateProductAsync(int productId)
        {
            const string sql = "SELECT p.*, u.* FROM dbo.Products p JOIN dbo.Users u ON u.Id = p.SellerId WHERE p.Id = @productId;";
            return (await connection.QueryAsync<PrivateProduct, Seller, PrivateProduct>(sql, (p, u) =>
            {
                p.Seller = u;
                return p;
            }, new { productId })).FirstOrDefault();
        }

        public async Task UpdateStatusAsync(ChangeProductStatusParams @params)
        {
            const string sql = "UPDATE dbo.Products SET Status = @Status, StatusUpdateDate = SYSDATETIME(), AdminId = @AdminId WHERE Id = @ProductId;";
            await connection.ExecuteAsync(sql, @params);
        }

        public async Task<bool> CanReviewProductAsync(int productId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.Products p LEFT JOIN dbo.ProductCustomers c ON p.Id = c.ProductId WHERE p.Id = @productId " +
                "AND (p.Price <= 0 OR c.UserId = @userId);";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productId, userId });
        }

        public async Task<bool> IsProductReviewOwnerAsync(int reviewId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.ProductReviews WHERE Id = @reviewId AND UserId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { reviewId, userId });
        }

        public async Task<bool> IsProductMediaSellerAsync(int mediaId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.ProductMedias m JOIN dbo.Products p ON p.Id = m.ProductId " +
                "WHERE m.Id = @mediaId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { mediaId, userId });
        }

        public async Task<bool> IsProductTabSellerAsync(int tabId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.ProductTabs t JOIN dbo.Products p ON p.Id = t.ProductId " +
                "WHERE t.Id = @tabId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { tabId, userId });
        }

        public async Task<bool> IsProductCustomerSellerAsync(int customerId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.ProductCustomers c JOIN dbo.Products p ON p.Id = c.ProductId " +
                "WHERE c.Id = @customerId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { customerId, userId });
        }

        public async Task<bool> IsProductSellerAsync(int productId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.Products WHERE Id = @productId AND SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productId, userId });
        }

        public async Task<int> GetProductImageIdAsync(int productId)
        {
            const string sql = "SELECT ImageId FROM dbo.Products WHERE Id = @productId;";
            return await connection.ExecuteScalarAsync<int>(sql, new { productId });
        }

        public async Task<IEnumerable<MProduct>> GetProductsAsync(int userId)
        {
            return await connection.QueryAsync<MProduct, Seller, MProduct>("dbo.GetProducts", (p, u) => 
            {
                p.Seller = u;
                return p;
            }, new { UserId = userId }, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<MProduct>> GetUserProductsAsync(int userId)
        {
            return await connection.QueryAsync<MProduct>("dbo.GetUserProducts", new { UserId = userId }, commandType: CommandType.StoredProcedure);
        }

        public async Task<MProduct> GetProductAsync(int productId, int userId)
        {
            const string sql = "dbo.GetProduct";

            MProduct product = (await connection.QueryAsync<MProduct, Seller, MProduct>(sql, (p, s) => 
            {
                p.Seller = s;
                return p;
            }, new { ProductId = productId }, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            if (product == null)
                return null;

            const string sql0 = "SELECT * FROM dbo.ProductTabs WHERE ProductId = @Id;";
            product.Tabs = (await connection.QueryAsync<MProductTab>(sql0, product)).ToList();

            const string sql1 = "SELECT * FROM dbo.ProductMedias WHERE ProductId = @Id;";
            product.Medias = (await connection.QueryAsync<MProductMedia>(sql1, product)).ToList();

            const string sql2 = "SELECT b.*, v.Id, v.Name, v.Changelog, v.DownloadsCount, v.IsEnabled, v.CreateDate FROM dbo.Branches b " +
                "LEFT JOIN dbo.Versions v ON v.BranchId = b.Id AND v.IsEnabled = 1 WHERE b.ProductId = @Id AND b.IsEnabled = 1;";

            product.Branches = new List<MBranch>();

            await connection.QueryAsync<MBranch, MVersion, MBranch>(sql2, (b, v) => 
            {
                var branch = product.Branches.FirstOrDefault(x => x.Id == b.Id);
                if (branch == null)
                {
                    branch = b;
                    branch.Versions = new List<MVersion>();
                    product.Branches.Add(branch);
                }

                if (v != null)
                {
                    branch.Versions.Add(v);
                }

                return null;
            }, product);

            if (userId != 0)
            {
                const string sql4 = "SELECT * FROM dbo.ProductCustomers WHERE ProductId = @productId AND UserId = @userId;";
                product.Customer = await connection.QuerySingleOrDefaultAsync<UserInfo>(sql4, new { productId, userId });
            }

            const string sql5 = "SELECT r.*, u.* FROM dbo.ProductReviews r JOIN dbo.Users u ON u.Id = r.UserId WHERE ProductId = @productId;";
            product.Reviews = (await connection.QueryAsync<MProductReview, UserInfo, MProductReview>(sql5, (r, u) =>
            {
                r.User = u;
                return r;
            }, new { productId })).ToList();

            return product;
        }

        public async Task<MProduct> AddProductAsync(MProduct product)
        {
            const string sql = "INSERT INTO dbo.Products (Name, Description, Category, GithubUrl, Price, ImageId, SellerId, IsEnabled, " +
                "Status, IsLoaderEnabled) " +
                "OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Description, INSERTED.Category, INSERTED.GithubUrl, INSERTED.Price, " +
                "INSERTED.ImageId, INSERTED.SellerId, INSERTED.IsEnabled, INSERTED.Status, INSERTED.IsLoaderEnabled, " +
                "INSERTED.LastUpdate, INSERTED.CreateDate " +
                "VALUES (@Name, @Description, @Category, @GithubUrl, @Price, @ImageId, @SellerId, @IsEnabled, @Status, @IsLoaderEnabled);";
            product = await connection.QuerySingleAsync<MProduct>(sql, product);

            const string sql1 = "INSERT INTO dbo.Branches (ProductId, Name, Description) " +
                "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.Name, INSERTED.Description, INSERTED.IsEnabled, INSERTED.CreateDate " +
                "VALUES (@Id, 'master', 'Default branch');";

            product.Branches = new List<MBranch>
            {
                await connection.QuerySingleAsync<MBranch>(sql1, product)
            };

            return product;
        }

        public async Task UpdateProductAsync(MProduct product)
        {
            const string sql = "UPDATE dbo.Products SET Name = @Name, Description = @Description, Category = @Category, GithubUrl = @GithubUrl, " +
                "Price = @Price, ImageId = @ImageId, IsEnabled = @IsEnabled, LastUpdate = SYSDATETIME() WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, product);
        }

        public async Task<MProductTab> AddProductTabAsync(MProductTab tab)
        {
            const string sql = "INSERT INTO dbo.ProductTabs (ProductId, Title, Content, IsEnabled) " +
                "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.Title, INSERTED.Content, INSERTED.IsEnabled " +
                "VALUES (@ProductId, @Title, @Content, @IsEnabled);";
            return await connection.QuerySingleAsync<MProductTab>(sql, tab);
        }

        public async Task UpdateProductTabAsync(MProductTab tab)
        {
            const string sql = "UPDATE dbo.ProductTabs SET Title = @Title, Content = @Content, IsEnabled = @IsEnabled " +
                "WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, tab);
        }

        public async Task DeleteProductTabAsync(int tabId)
        {
            const string sql = "DELETE FROM dbo.ProductTabs WHERE Id = @tabId;";
            await connection.ExecuteAsync(sql, new { tabId });
        }

        public async Task<MProductMedia> AddProductMediaAsync(MProductMedia media)
        {
            const string sql = "INSERT INTO dbo.ProductMedias (ProductId, YoutubeUrl, ImageId) " +
                "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.YoutubeUrl, INSERTED.ImageId " +
                "VALUES (@ProductId, @YoutubeUrl, @ImageId);";
            return await connection.QuerySingleAsync<MProductMedia>(sql, media);
        }

        public async Task DeleteProductMediaAsync(int mediaId)
        {
            const string sql = "DELETE FROM dbo.ProductMedias WHERE Id = @mediaId;";
            await connection.ExecuteAsync(sql, new { mediaId });
        }

        public async Task<MProductCustomer> AddProductCustomerAsync(MProductCustomer customer)
        {
            const string sql = "INSERT INTO dbo.ProductCustomers (ProductId, UserId) " +
                "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.UserId, INSERTED.CreateDate " +
                "VALUES (@ProductId, @UserId);";

            return await connection.QuerySingleAsync<MProductCustomer>(sql, customer);
        }

        public async Task DeleteProductCustomerAsync(int customerId)
        {
            const string sql = "DELETE FROM dbo.ProductCustomers WHERE Id = @customerId;";
            await connection.ExecuteAsync(sql, new { customerId });
        }

        public async Task<MProductReview> AddProductReviewAsync(MProductReview review)
        {
            const string sql = "INSERT INTO dbo.ProductReviews (Title, Body, Rating, ProductId, UserId) " +
                "OUTPUT INSERTED.Id, INSERTED.Title, INSERTED.Body, INSERTED.Rating, INSERTED.ProductId, INSERTED.UserId, " +
                "INSERTED.LastUpdate, INSERTED.CreateDate " +
                "VALUES (@Title, @Body, @Rating, @ProductId, @UserId);";
            return await GetProductReviewAsync(await connection.ExecuteScalarAsync<int>(sql, review));
        }

        public async Task<MProductReview> GetProductReviewAsync(int reviewId)
        {
            const string sql = "SELECT r.*, u.* FROM dbo.ProductReviews r JOIN dbo.Users u ON u.Id = r.UserId " +
                "WHERE r.Id = @reviewId;";

            return (await connection.QueryAsync<MProductReview, UserInfo, MProductReview>(sql, (r, u) => 
            {
                r.User = u;
                return r;
            }, new { reviewId })).FirstOrDefault();
        }

        public async Task UpdateProductReviewAsync(MProductReview review)
        {
            const string sql = "UPDATE dbo.ProductReviews SET Title = @Title, Body = @Body, Rating = @Rating, " +
                "LastUpdate = SYSDATETIME() WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, review);
        }

        public async Task DeleteProductReviewAsync(int reviewId)
        {
            const string sql = "DELETE FROM dbo.ProductReviews WHERE Id = @reviewId;";
            await connection.ExecuteAsync(sql, new { reviewId });
        }

        public async Task<IEnumerable<MProductCustomer>> GetMyProductsAsync(int userId)
        {
            const string sql = "SELECT c.*, p.*, u.* FROM dbo.ProductCustomers c JOIN dbo.Products p on c.ProductId = p.Id " +
                "JOIN dbo.Users u ON p.SellerId = u.Id WHERE c.UserId = @userId;";

            return await connection.QueryAsync<MProductCustomer, MProduct, Seller, MProductCustomer>(sql, (c, p, u) => 
            {
                c.Product = p;
                c.Product.Seller = u;
                return c;
            }, new { userId });
        }
    }
}
