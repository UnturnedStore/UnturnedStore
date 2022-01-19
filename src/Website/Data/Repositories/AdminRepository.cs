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
            const string sql = "SELECT p.*, u.* FROM dbo.Products p JOIN dbo.Users u ON p.SellerId = u.Id;";
            return await connection.QueryAsync<MProduct, Seller, MProduct>(sql, (p, s) => 
            {
                p.Seller = s;
                return p;
            });
        }
    }
}
