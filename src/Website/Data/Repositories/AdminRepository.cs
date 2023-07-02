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
    public class AdminRepository
    {
        private readonly SqlConnection connection;

        public AdminRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<IEnumerable<MUser>> GetUsersAsync()
        {
            const string sql = "SELECT * FROM dbo.Users;";
            return await connection.QueryAsync<MUser>(sql);
        }

        public async Task UpdateUserAsync(MUser user)
        {
            const string sql = "UPDATE dbo.Users SET Role = @Role, IsVerifiedSeller = @IsVerifiedSeller WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, user);
        }

        public async Task<IEnumerable<MProduct>> GetProductsAsync()
        {
            const string sql = "SELECT p.*, u.*, t.*, ps.* FROM dbo.Products p " +
                "JOIN dbo.Users u ON p.SellerId = u.Id " +
                "LEFT JOIN dbo.Tags t ON t.Id IN (SELECT TagId FROM dbo.ProductTags WHERE ProductId = p.Id)" +
                "LEFT JOIN dbo.ProductSales ps ON ps.ProductId = p.Id AND ps.IsActive = 1 AND ps.IsExpired = 0;";

            var productDictionary = new Dictionary<int, MProduct>();

            return (await connection.QueryAsync<MProduct, Seller, MProductTag, MProductSale, MProduct>(sql, (p, s, t, ps) => 
            {
                if (!productDictionary.TryGetValue(p.Id, out MProduct mappedProduct))
                {
                    mappedProduct = p;
                    mappedProduct.Seller = s;
                    mappedProduct.Tags = new List<MProductTag>();
                    mappedProduct.Sale = ps;
                    productDictionary.Add(mappedProduct.Id, mappedProduct);
                }

                mappedProduct.Tags.Add(t);
                return mappedProduct;
            })).Distinct();
        }
    }
}
