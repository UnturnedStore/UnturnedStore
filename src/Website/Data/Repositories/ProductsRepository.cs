using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Data.Repositories
{
    public class ProductsRepository
    {
        private readonly SqlConnection connection;

        public ProductsRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<bool> IsProductMediaSellerAsync(int mediaId, int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.ProductMedias m JOIN dbo.Products p ON p.Id = m.ProductId " +
                "WHERE m.Id = @mediaId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { mediaId, userId });
        }

        public async Task<bool> IsProductTabSellerAsync(int tabId, int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.ProductTabs t JOIN dbo.Products p ON p.Id = t.ProductId " +
                "WHERE t.Id = @tabId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { tabId, userId });
        }

        public async Task<bool> IsProductCustomerSellerAsync(int productId, int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.ProductCustomers c JOIN dbo.Products p ON p.Id = c.ProductId " +
                "WHERE c.Id = @productId aND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productId, userId });
        }

        public async Task<bool> IsProductSellerAsync(int productId, int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Products WHERE Id = @productId AND SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { productId, userId });
        }

        public async Task<IEnumerable<ProductModel>> GetProductsAsync()
        {
            const string sql = "SELECT p.*, u.Id, u.Name, u.Role, u.SteamId, u.CreateDate FROM dbo.Products p JOIN dbo.Users u ON p.SellerId = u.Id WHERE IsEnabled = 1;";
            var products = await connection.QueryAsync<ProductModel, UserModel, ProductModel>(sql, (p, u) => 
            {
                p.Seller = u;
                return p;
            });

            const string sql2 = "SELECT COALESCE(SUM(v.DownloadsCount), 0) FROM dbo.Branches b " +
                "LEFT JOIN dbo.Versions v ON v.BranchId = b.Id WHERE b.ProductId = @Id AND v.IsEnabled = 1;";
            foreach (var product in products)
            {
                product.TotalDownloadsCount = await connection.ExecuteScalarAsync<int>(sql2, product);
            }
            return products;
        }

        public async Task<ProductModel> GetProductAsync(int productId, int userId)
        {
            const string sql = "SELECT p.*, t.* FROM dbo.Products p LEFT JOIN dbo.ProductTabs t ON p.Id = t.ProductId WHERE p.Id = @productId;";
            
            ProductModel product = null;
            await connection.QueryAsync<ProductModel, ProductTabModel, ProductModel>(sql, (p, t) => 
            { 
                if (product == null)
                {
                    product = p;
                    product.Tabs = new List<ProductTabModel>();
                }

                if (t != null)
                    product.Tabs.Add(t);

                return null;
            }, new { productId });

            if (product == null)
                return null;

            const string sql1 = "SELECT * FROM dbo.ProductMedias WHERE ProductId = @Id;";
            product.Medias = (await connection.QueryAsync<ProductMediaModel>(sql1, product)).ToList();

            const string sql2 = "SELECT b.*, v.Id, v.Name, v.Changelog, v.DownloadsCount, v.IsEnabled, v.CreateDate FROM dbo.Branches b " +
                "LEFT JOIN dbo.Versions v ON v.BranchId = b.Id AND v.IsEnabled = 1 WHERE b.ProductId = @Id AND b.IsEnabled = 1;";

            product.Branches = new List<BranchModel>();

            await connection.QueryAsync<BranchModel, VersionModel, BranchModel>(sql2, (b, v) => 
            {
                var branch = product.Branches.FirstOrDefault(x => x.Id == b.Id);
                if (branch == null)
                {
                    branch = b;
                    branch.Versions = new List<VersionModel>();
                    product.Branches.Add(branch);
                }

                if (v != null)
                {
                    branch.Versions.Add(v);
                }

                return null;
            }, product);

            const string sql3 = "SELECT Id, Name, Role, SteamId, CreateDate FROM dbo.Users WHERE Id = @SellerId;";
            product.Seller = await connection.QuerySingleAsync<UserModel>(sql3, product);

            if (userId != 0)
            {
                const string sql4 = "SELECT * FROM dbo.ProductCustomers WHERE ProductId = @productId AND UserId = @userId;";
                product.Customer = await connection.QuerySingleOrDefaultAsync<UserModel>(sql4, new { productId, userId });
            }

            return product;
        }

        public async Task<ProductModel> AddProductAsync(ProductModel product)
        {
            const string sql = "INSERT INTO dbo.Products (Name, Description, GithubUrl, Price, ImageId, SellerId, IsEnabled) " +
                "OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Description, INSERTED.GithubUrl, INSERTED.Price, " +
                "INSERTED.ImageId, INSERTED.SellerId, INSERTED.IsEnabled, INSERTED.LastUpdate, INSERTED.CreateDate " +
                "VALUES (@Name, @Description, @GithubUrl, @Price, @ImageId, @SellerId, @IsEnabled);";
            product = await connection.QuerySingleAsync<ProductModel>(sql, product);

            const string sql1 = "INSERT INTO dbo.Branches (ProductId, Name, Description) " +
                "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.Name, INSERTED.Description, INSERTED.IsEnabled, INSERTED.CreateDate " +
                "VALUES (@Id, 'master', 'Default branch');";

            product.Branches = new List<BranchModel>
            {
                await connection.QuerySingleAsync<BranchModel>(sql1, product)
            };

            return product;
        }

        public async Task UpdateProductAsync(ProductModel product)
        {
            const string sql = "UPDATE dbo.Products SET Name = @Name, Description = @Description, GithubUrl = @GithubUrl, " +
                "Price = @Price, ImageId = @ImageId, IsEnabled = @IsEnabled, LastUpdate = SYSDATETIME() WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, product);
        }

        public async Task<ProductTabModel> AddProductTabAsync(ProductTabModel tab)
        {
            const string sql = "INSERT INTO dbo.ProductTabs (ProductId, Title, Content, IsEnabled) " +
                "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.Title, INSERTED.Content, INSERTED.IsEnabled " +
                "VALUES (@ProductId, @Title, @Content, @IsEnabled);";
            return await connection.QuerySingleAsync<ProductTabModel>(sql, tab);
        }

        public async Task UpdateProductTabAsync(ProductTabModel tab)
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

        public async Task<ProductMediaModel> AddProductMediaAsync(ProductMediaModel media)
        {
            const string sql = "INSERT INTO dbo.ProductMedias (ProductId, YoutubeUrl, ImageId) " +
                "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.YoutubeUrl, INSERTED.ImageId " +
                "VALUES (@ProductId, @YoutubeUrl, @ImageId);";
            return await connection.QuerySingleAsync<ProductMediaModel>(sql, media);
        }

        public async Task DeleteProductMediaAsync(int mediaId)
        {
            const string sql = "DELETE FROM dbo.ProductMedias WHERE Id = @mediaId;";
            await connection.ExecuteAsync(sql, new { mediaId });
        }

        public async Task<ProductCustomerModel> AddProductCustomerAsync(ProductCustomerModel customer)
        {
            const string sql = "INSERT INTO dbo.ProductCustomers (ProductId, UserId) " +
                "OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.UserId, INSERTED.CreateDate " +
                "VALUES (@ProductId, @UserId);";

            return await connection.QuerySingleAsync<ProductCustomerModel>(sql, customer);
        }

        public async Task DeleteProductCustomerAsync(int customerId)
        {
            const string sql = "DELETE FROM dbo.ProductCustomers WHERE Id = @customerId;";
            await connection.ExecuteAsync(sql, new { customerId });
        }

        public async Task<IEnumerable<ProductCustomerModel>> GetUserProductsAsync(int userId)
        {
            const string sql = "SELECT c.*, p.*, u.Id, u.Name, u.Role, u.SteamId, u.CreateDate FROM dbo.ProductCustomers c JOIN dbo.Products p on c.ProductId = p.Id " +
                "JOIN dbo.Users u ON p.SellerId = u.Id WHERE c.UserId = @userId;";

            return await connection.QueryAsync<ProductCustomerModel, ProductModel, UserModel, ProductCustomerModel>(sql, (c, p, u) => 
            {
                c.Product = p;
                c.Product.Seller = u;
                return c;
            }, new { userId });
        }
    }
}
