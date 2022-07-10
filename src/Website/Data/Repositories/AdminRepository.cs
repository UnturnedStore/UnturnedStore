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
            const string sql = "SELECT p.*, u.*, t.* FROM dbo.Products p " +
                "JOIN dbo.Users u ON p.SellerId = u.Id " +
                "LEFT JOIN dbo.Tags t ON t.Id IN (SELECT TagId FROM dbo.ProductTags WHERE ProductId = p.Id);";

            var productDictionary = new Dictionary<int, MProduct>();

            return (await connection.QueryAsync<MProduct, Seller, MProductTag, MProduct>(sql, (p, s, t) => 
            {
                if (!productDictionary.TryGetValue(p.Id, out MProduct mappedProduct))
                {
                    mappedProduct = p;
                    mappedProduct.Seller = s;
                    mappedProduct.Tags = new List<MProductTag>();
                    productDictionary.Add(mappedProduct.Id, mappedProduct);
                }

                mappedProduct.Tags.Add(t);
                return mappedProduct;
            })).Distinct();
        }
    }
}
