using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Extensions;
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

        public async Task SetProductReleaseDateAsync(int productId, DateTime releaseDate)
        {
            const string sql = "UPDATE dbo.Products SET ReleaseDate = @releaseDate WHERE Id = @productId;";
            await connection.ExecuteAsync(sql, new { productId, releaseDate });
        }

        public async Task UpdateStatusAsync(ChangeProductStatusParams @params)
        {
            const string sql = "UPDATE dbo.Products SET Status = @Status, StatusReason = @StatusReason, StatusUpdateDate = SYSDATETIME(), AdminId = @AdminId WHERE Id = @ProductId;";
            await connection.ExecuteAsync(sql, @params);
        }

        public async Task<bool> CanReviewProductAsync(int productId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.Products p LEFT JOIN dbo.ProductCustomers c ON p.Id = c.ProductId WHERE p.Id = @productId " +
                "AND (p.Price <= 0 OR c.UserId = @userId);";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productId, userId });
        }

        public async Task<bool> CanReplyReviewAsync(int reviewId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.Products p WHERE p.Id = (SELECT ProductId FROM dbo.ProductReviews r WHERE r.Id = @reviewId) AND " +
                "@reviewId NOT IN (SELECT ReviewId FROM dbo.ProductReviewReplies) AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { reviewId, userId });
        }

        public async Task<bool> IsProductReviewOwnerAsync(int reviewId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.ProductReviews WHERE Id = @reviewId AND UserId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { reviewId, userId });
        }

        public async Task<bool> IsProductReviewReplyOwnerAsync(int replyId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.ProductReviewReplies WHERE Id = @replyId AND UserId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { replyId, userId });
        }

        public async Task<bool> IsProductMediaSellerAsync(int mediaId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.ProductMedias m JOIN dbo.Products p ON p.Id = m.ProductId " +
                "WHERE m.Id = @mediaId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { mediaId, userId });
        }

        public async Task<bool> IsProductWorkshopItemSellerAsync(int workshopId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.ProductWorkshops w JOIN dbo.Products p ON p.Id = w.ProductId " +
                "WHERE w.Id = @workshopId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { workshopId, userId });
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
            var productDictionary = new Dictionary<int, MProduct>();

            IEnumerable<MProduct> products = (await connection.QueryAsync<MProduct, Seller, MProductTag, MProductSale, MProduct>("dbo.GetProducts", (p, u, t, ps) => 
            {
                if (!productDictionary.TryGetValue(p.Id, out MProduct mappedProduct))
                {
                    mappedProduct = p;
                    mappedProduct.Seller = u;
                    mappedProduct.Tags = new List<MProductTag>();
                    mappedProduct.Sale = ps;

                    productDictionary.Add(mappedProduct.Id, mappedProduct);
                }

                mappedProduct.Tags.Add(t);
                return mappedProduct;
            }, new { UserId = userId }, commandType: CommandType.StoredProcedure)).Distinct();

            return products;
        }

        public async Task<IEnumerable<MProduct>> GetUserProductsAsync(int userId)
        {
            IEnumerable<MProduct> products = await connection.QueryAsync<MProduct, MProductSale, MProduct>("dbo.GetUserProducts", (p, ps) => 
            {
                p.Sale = ps;
                return p;
            }, new { UserId = userId }, commandType: CommandType.StoredProcedure);

            return products;
        }

        public async Task<MProduct> GetProductAsync(int productId, int userId)
        {
            const string sql = "dbo.GetProduct";

            MProduct product = (await connection.QueryAsync<MProduct, Seller, MProductSale, MProduct>(sql, (p, s, ps) => 
            {
                p.Seller = s;
                p.Sale = ps;
                return p;
            }, new { ProductId = productId }, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            if (product == null)
                return null;

            const string sql0 = "SELECT * FROM dbo.Tags WHERE Id IN (SELECT TagId FROM dbo.ProductTags WHERE ProductId = @Id);";
            product.Tags = (await connection.QueryAsync<MProductTag>(sql0, product)).ToList();

            const string sql1 = "SELECT * FROM dbo.ProductTabs WHERE ProductId = @Id;";
            product.Tabs = (await connection.QueryAsync<MProductTab>(sql1, product)).ToList();

            const string sql2 = "SELECT * FROM dbo.ProductMedias WHERE ProductId = @Id;";
            product.Medias = (await connection.QueryAsync<MProductMedia>(sql2, product)).ToList();

            const string sql3 = "SELECT * FROM dbo.ProductWorkshops WHERE ProductId = @Id;";
            product.WorkshopItems = (await connection.QueryAsync<MProductWorkshopItem>(sql3, product)).ToList();

            const string sql4 = "SELECT b.*, v.Id, v.Name, v.Changelog, v.DownloadsCount, v.IsEnabled, v.CreateDate FROM dbo.Branches b " +
                "LEFT JOIN dbo.Versions v ON v.BranchId = b.Id AND v.IsEnabled = 1 WHERE b.ProductId = @Id AND b.IsEnabled = 1;";

            product.Branches = new List<MBranch>();

            await connection.QueryAsync<MBranch, MVersion, MBranch>(sql4, (b, v) => 
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
                const string sql5 = "SELECT * FROM dbo.ProductCustomers WHERE ProductId = @productId AND UserId = @userId;";
                product.Customer = await connection.QuerySingleOrDefaultAsync<UserInfo>(sql5, new { productId, userId });
            }

            const string sql6 = "SELECT r.*, u.*, rp.*, rpu.* FROM dbo.ProductReviews r JOIN dbo.Users u ON u.Id = r.UserId " + 
                "LEFT JOIN dbo.ProductReviewReplies rp ON rp.ReviewId = r.Id LEFT JOIN dbo.Users rpu ON rpu.Id = rp.UserId WHERE ProductId = @productId;";

            product.Reviews = (await connection.QueryAsync<MProductReview, UserInfo, MProductReviewReply, UserInfo, MProductReview>(sql6, (r, u, rp, rpu) =>
            {
                r.User = u;

                if (rp != null)
                {
                    rp.User = rpu;
                    r.Reply = rp;
                }

                return r;
            }, new { productId })).ToList();

            return product;
        }

        public async Task<MProduct> AddProductAsync(MProduct product)
        {
            var initialTags = product.Tags;
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

            product.Tags = await SetProductTagsAsync(product.Id, initialTags);

            return product;
        }

        public async Task UpdateProductAsync(MProduct product)
        {
            const string sql = "UPDATE dbo.Products SET Name = @Name, Description = @Description, Category = @Category, GithubUrl = @GithubUrl, " +
                "Price = @Price, ImageId = @ImageId, IsEnabled = @IsEnabled, LastUpdate = SYSDATETIME() WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, product);

            await SetProductTagsAsync(product.Id, product.Tags);
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

        public async Task<List<MProductTag>> GetTagsAsync()
        {
            const string sql = "SELECT * FROM dbo.Tags;";
            return (await connection.QueryAsync<MProductTag>(sql)).ToList();
        }

        public async Task<MProductTag> AddTagAsync(MProductTag tag)
        {
            const string sql = "INSERT INTO dbo.Tags (Title, Color, BackgroundColor) " +
                "OUTPUT INSERTED.Id, INSERTED.Title, INSERTED.Color, INSERTED.BackgroundColor " +
                "VALUES (@Title, @Color, @BackgroundColor);";
            return await connection.QuerySingleAsync<MProductTag>(sql, tag);
        }

        public async Task<List<MProductTag>> SetProductTagsAsync(int productId, List<MProductTag> tags)
        {
            if (tags == null) return new List<MProductTag>();

            const string sql0 = "DELETE FROM dbo.ProductTags WHERE ProductId = @productId;";
            await connection.ExecuteAsync(sql0, new { productId });

            if (tags.Count == 0) return new List<MProductTag>();

            string sql1 = $"INSERT INTO dbo.ProductTags (ProductId, TagId) VALUES {ProductTagsInsertMultiple(tags)};";
            await connection.ExecuteAsync(sql1, new { productId });
            return tags;
        }

        private static string ProductTagsInsertMultiple(List<MProductTag> tags)
        {
            List<string> tagsInsert = new List<string>();

            foreach (MProductTag tag in tags)
                tagsInsert.Add($"(@productId, {tag.Id})");

            return string.Join(",", tagsInsert);
        }

        public async Task UpdateTagAsync(MProductTag tag)
        {
            const string sql = "UPDATE dbo.Tags SET Title = @Title, Color = @Color, BackgroundColor = @BackgroundColor " +
                "WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, tag);
        }

        public async Task DeleteTagAsync(int tagId)
        {
            const string sql = "DELETE FROM dbo.Tags WHERE Id = @tagId;";
            await connection.ExecuteAsync(sql, new { tagId });
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
                "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.UserId, INSERTED.CreateDate, INSERTED.LicenseKey " + // Auto getting LicenseKey on AddCustomerModel (made special to prevert reloading page)
                "VALUES (@ProductId, @UserId);";

            return await connection.QuerySingleAsync<MProductCustomer>(sql, customer);
        }

        public async Task UpdateProductCustomerAsync(MProductCustomer customer)
        {
            const string sql = "UPDATE dbo.ProductCustomers SET IsBlocked = @IsBlocked, " +
                "BlockDate = @BlockDate WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, customer);
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

        // Doesnt Support Review Replies since this function is only used once by AddProductReviewAsync
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

        public async Task<MProductReviewReply> AddProductReviewReplyAsync(MProductReviewReply reply)
        {
            const string sql = "INSERT INTO dbo.ProductReviewReplies (Body, ReviewId, UserId) " +
                "OUTPUT INSERTED.Id, INSERTED.Body, INSERTED.ReviewId, INSERTED.UserId, INSERTED.LastUpdate, INSERTED.CreateDate " +
                "VALUES (@Body, @ReviewId, @UserId);";

            return await connection.QuerySingleAsync<MProductReviewReply>(sql, reply);
        }

        public async Task UpdateProductReviewReplyAsync(MProductReviewReply reply)
        {
            const string sql = "UPDATE dbo.ProductReviewReplies SET Body = @Body, LastUpdate = SYSDATETIME() WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, reply);
        }

        public async Task DeleteProductReviewReplyAsync(int replyId)
        {
            const string sql = "DELETE FROM dbo.ProductReviewReplies WHERE Id = @replyId;";
            await connection.ExecuteAsync(sql, new { replyId });
        }

        public async Task<MProductWorkshopItem> AddProductWorkshopItemAsync(MProductWorkshopItem workshopItem)
        {
            const string sql = "INSERT INTO dbo.ProductWorkshops (ProductId, DatabaseFileId, IsRequired) " +
                "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.DatabaseFileId, INSERTED.IsRequired " +
                "VALUES (@ProductId, @DatabaseFileId, @IsRequired);";
            return await connection.QuerySingleAsync<MProductWorkshopItem>(sql, workshopItem);
        }

        public async Task UpdateProductWorkshopItemAsync(MProductWorkshopItem workshopItem)
        {
            const string sql = "UPDATE dbo.ProductWorkshops SET DatabaseFileId = @DatabaseFileId, IsRequired = @IsRequired " +
                "WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, workshopItem);
        }

        public async Task DeleteProductWorkshopItemAsync(int workshopId)
        {
            const string sql = "DELETE FROM dbo.ProductWorkshops WHERE Id = @workshopId;";
            await connection.ExecuteAsync(sql, new { workshopId });
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
