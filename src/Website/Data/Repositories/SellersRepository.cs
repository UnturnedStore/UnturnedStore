using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models;
using Website.Shared.Models.Database;

namespace Website.Data.Repositories
{
    public class SellersRepository
    {
        private readonly SqlConnection connection;

        public SellersRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<IEnumerable<MOrder>> GetOrdersAsync(int sellerId)
        {
            const string sql = "SELECT o.*, u.*, i.*, p.* FROM dbo.Orders o JOIN dbo.Users u ON o.BuyerId = u.Id " +
                "LEFT JOIN dbo.OrderItems i ON o.Id = i.OrderId JOIN dbo.Products p ON i.ProductId = p.Id WHERE o.SellerId = @sellerId AND Status = 'Completed';";

            List<MOrder> orders = new List<MOrder>();
            await connection.QueryAsync<MOrder, UserInfo, MOrderItem, MProduct, MOrder>(sql, (o, u, i, p) =>
            {
                var order = orders.FirstOrDefault(x => x.Id == o.Id);
                if (order == null)
                {
                    order = o;
                    order.Buyer = u;
                    order.Items = new List<MOrderItem>();
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

        public async Task<IEnumerable<MProductCustomer>> GetCustomersAsync(int userId)
        {
            const string sql = "SELECT c.*, u.*, p.* FROM dbo.ProductCustomers c JOIN dbo.Users u ON u.Id = c.UserId " +
                "JOIN dbo.Products p ON p.Id = c.ProductId WHERE p.SellerId = @userId;";

            return await connection.QueryAsync<MProductCustomer, UserInfo, MProduct, MProductCustomer>(sql, (c, u, p) => 
            {
                c.User = u;
                c.Product = p;                
                return c;
            }, new { userId });
        }

        public async Task<IEnumerable<MProduct>> GetProductsAsync(int userId)
        {
            const string sql = "SELECT p.*, b.* FROM dbo.Products p LEFT JOIN dbo.Branches b ON p.Id = b.ProductId WHERE p.SellerId = @userId;";

            List<MProduct> products = new List<MProduct>();
            await connection.QueryAsync<MProduct, MBranch, MProduct>(sql, (p, b) =>
            {
                var product = products.FirstOrDefault(x => x.Id == p.Id);
                if (product == null)
                {
                    product = p;
                    product.Branches = new List<MBranch>();
                    products.Add(product);
                }

                if (b != null)
                    product.Branches.Add(b);

                return null;
            }, new { userId });

            return products;
        }

        public async Task<MProduct> GetProductAsync(int productId)
        {
            const string sql = "SELECT p.*, t.* FROM dbo.Products p LEFT JOIN dbo.ProductTabs t ON p.Id = t.ProductId WHERE p.Id = @productId;";

            MProduct product = null;
            await connection.QueryAsync<MProduct, MProductTab, MProduct>(sql, (p, t) =>
            {
                if (product == null)
                {
                    product = p;
                    product.Tabs = new List<MProductTab>();
                }

                if (t != null)
                    product.Tabs.Add(t);

                return null;
            }, new { productId });

            if (product == null)
                return product;

            const string sql1 = "SELECT * FROM dbo.ProductMedias WHERE ProductId = @Id;";
            product.Medias = (await connection.QueryAsync<MProductMedia>(sql1, product)).ToList();

            const string sql2 = "SELECT b.*, v.Id, v.BranchId, v.Name, v.FileName, v.Changelog, v.DownloadsCount, v.IsEnabled, v.CreateDate " +
                "FROM dbo.Branches b LEFT JOIN dbo.Versions v ON v.BranchId = b.Id WHERE b.ProductId = @Id;";
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
