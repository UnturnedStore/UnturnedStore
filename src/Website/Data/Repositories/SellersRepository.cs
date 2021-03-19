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
    public class SellersRepository
    {
        private readonly SqlConnection connection;

        public SellersRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<IEnumerable<OrderModel>> GetOrdersAsync(int sellerId)
        {
            const string sql = "SELECT o.*, u.*, i.*, p.* FROM dbo.Orders o JOIN dbo.Users u ON o.BuyerId = u.Id " +
                "LEFT JOIN dbo.OrderItems i ON o.Id = i.OrderId JOIN dbo.Products p ON i.ProductId = p.Id WHERE o.SellerId = @sellerId AND Status = 'Completed';";

            List<OrderModel> orders = new List<OrderModel>();
            await connection.QueryAsync<OrderModel, UserModel, OrderItemModel, ProductModel, OrderModel>(sql, (o, u, i, p) =>
            {
                var order = orders.FirstOrDefault(x => x.Id == o.Id);
                if (order == null)
                {
                    order = o;
                    order.Buyer = u;
                    order.Items = new List<OrderItemModel>();
                    orders.Add(order);
                }

                if (i != null)
                {
                    i.Product = p;
                    order.Items.Add(i);
                }

                return null;
            }, new { sellerId });

            return orders;
        }

        public async Task<IEnumerable<ProductCustomerModel>> GetCustomersAsync(int userId)
        {
            const string sql = "SELECT c.*, u.*, p.* FROM dbo.ProductCustomers c JOIN dbo.Users u ON u.Id = c.UserId " +
                "JOIN dbo.Products p ON p.Id = c.ProductId WHERE p.SellerId = @userId;";

            return await connection.QueryAsync<ProductCustomerModel, UserModel, ProductModel, ProductCustomerModel>(sql, (c, u, p) => 
            {
                c.User = u;
                c.Product = p;                
                return c;
            }, new { userId });
        }

        public async Task<IEnumerable<ProductModel>> GetProductsAsync(int userId)
        {
            const string sql = "SELECT p.*, b.* FROM dbo.Products p LEFT JOIN dbo.Branches b ON p.Id = b.ProductId WHERE p.SellerId = @userId;";

            List<ProductModel> products = new List<ProductModel>();
            await connection.QueryAsync<ProductModel, BranchModel, ProductModel>(sql, (p, b) =>
            {
                var product = products.FirstOrDefault(x => x.Id == p.Id);
                if (product == null)
                {
                    product = p;
                    product.Branches = new List<BranchModel>();
                    products.Add(product);
                }

                if (b != null)
                    product.Branches.Add(b);

                return null;
            }, new { userId });

            return products;
        }

        public async Task<ProductModel> GetProductAsync(int productId)
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

            const string sql1 = "SELECT * FROM dbo.ProductMedias WHERE ProductId = @Id;";
            product.Medias = (await connection.QueryAsync<ProductMediaModel>(sql1, product)).ToList();

            const string sql2 = "SELECT b.*, v.Id, v.BranchId, v.Name, v.FileName, v.Changelog, v.DownloadsCount, v.IsEnabled, v.CreateDate " +
                "FROM dbo.Branches b LEFT JOIN dbo.Versions v ON v.BranchId = b.Id WHERE b.ProductId = @Id;";
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

                if (v != null && v.Id != default)
                {
                    branch.Versions.Add(v);
                }

                return null;
            }, product);

            return product;
        }
    }
}
