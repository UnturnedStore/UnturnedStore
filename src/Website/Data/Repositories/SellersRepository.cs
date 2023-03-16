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
                "LEFT JOIN dbo.OrderItems i ON o.Id = i.OrderId JOIN dbo.Products p ON i.ProductId = p.Id WHERE o.SellerId = @sellerId AND o.Status = 'Completed';";

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
            const string sql = "SELECT p.*, b.*, ps.* FROM dbo.Products p LEFT JOIN dbo.Branches b ON p.Id = b.ProductId LEFT JOIN dbo.ProductSales ps ON p.Id = ps.ProductId AND ps.IsExpired = 0 AND ps.IsActive = 1 WHERE p.SellerId = @userId;";

            List<MProduct> products = new List<MProduct>();
            await connection.QueryAsync<MProduct, MBranch, MProductSale, MProduct>(sql, (p, b, ps) =>
            {
                var product = products.FirstOrDefault(x => x.Id == p.Id);
                if (product == null)
                {
                    product = p;
                    product.Branches = new List<MBranch>();
                    product.Sale = ps;
                    products.Add(product);
                }

                if (b != null)
                    product.Branches.Add(b);

                return null;
            }, new { userId });

            return products;
        }

        public async Task<SellerProduct> GetSellerProductAsync(int productId)
        {
            const string sql = "SELECT p.*, s.*, a.*, ps.*, t.* FROM dbo.Products p " +
                "JOIN dbo.Users s ON s.Id = p.SellerId " +
                "LEFT JOIN dbo.Users a ON a.Id = p.AdminId " +
                "LEFT JOIN dbo.ProductSales ps ON p.Id = ps.ProductId AND ps.IsExpired = 0 AND ps.IsActive = 1 " +
                "LEFT JOIN dbo.ProductTabs t ON p.Id = t.ProductId " +
                "WHERE p.Id = @productId;";

            SellerProduct product = null;
            await connection.QueryAsync<SellerProduct, Seller, UserInfo, MProductSale, MProductTab, SellerProduct>(sql, (p, s, a, ps, t) =>
            {
                if (product == null)
                {
                    product = p;
                    p.Seller = s;
                    p.Admin = a;
                    p.Sale = ps;
                    product.Tabs = new List<MProductTab>();
                }

                if (t != null)
                    product.Tabs.Add(t);

                return null;
            }, new { productId });

            if (product == null)
                return product;

            const string sql1 = "SELECT * FROM dbo.Tags WHERE Id IN (SELECT TagId FROM dbo.ProductTags WHERE ProductId = @Id);";
            product.Tags = (await connection.QueryAsync<MProductTag>(sql1, product)).ToList();

            const string sql2 = "SELECT * FROM dbo.ProductMedias WHERE ProductId = @Id;";
            product.Media = (await connection.QueryAsync<MProductMedia>(sql2, product)).ToList();

            const string sql3 = "SELECT * FROM dbo.ProductWorkshops WHERE ProductId = @Id;";
            product.WorkshopItems = (await connection.QueryAsync<MProductWorkshopItem>(sql3, product)).ToList();

            const string sql4 = "SELECT b.*, v.Id, v.BranchId, v.Name, v.FileName, v.Changelog, v.DownloadsCount, v.IsEnabled, v.CreateDate " +
                "FROM dbo.Branches b LEFT JOIN dbo.Versions v ON v.BranchId = b.Id WHERE b.ProductId = @Id;";
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
