using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Data.Repositories
{
    public class UsersRepository
    {
        private readonly SqlConnection connection;

        public UsersRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<UserModel> GetUserFullAsync(int userId)
        {
            const string sql = "SELECT u.*, c.*, p.* FROM dbo.Users u LEFT JOIN dbo.ProductCustomers c ON u.Id = c.UserId " +
                "LEFT JOIN dbo.Products p ON c.ProductId = p.Id WHERE u.Id = @userId;";

            UserModel user = null;

            await connection.QueryAsync<UserModel, ProductCustomerModel, ProductModel, UserModel>(sql, (u, c, p) => 
            { 
                if (user == null)
                {
                    user = u;
                    user.Products = new List<ProductCustomerModel>();
                }

                if (c != null)
                {
                    c.Product = p;
                    user.Products.Add(c);
                }                    

                return null;
            }, new { userId });

            return user;
        }

        public async Task<UserModel> GetUserAsync(int userId)
        {
            const string sql = "SELECT Id, Name, Role, SteamId, PayPalEmail, PayPalCurrency, CreateDate FROM dbo.Users WHERE Id = @userId;";
            return await connection.QuerySingleOrDefaultAsync<UserModel>(sql, new { userId });
        }
        
        public async Task<UserModel> GetUserAsync(string steamId)
        {
            const string sql = "SELECT Id, Name, Role, SteamId, CreateDate " +
                "FROM dbo.Users WHERE SteamId = @steamId;";

            return await connection.QuerySingleOrDefaultAsync<UserModel>(sql, new { steamId });
        }

        public async Task<UserModel> AddUserAsync(UserModel user)
        {
            const string sql = "INSERT INTO dbo.Users (Name, Role, SteamId) " +
                "OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Role, INSERTED.SteamId, INSERTED.CreateDate " +
                "VALUES (@Name, @Role, @SteamId);";

            return await connection.QuerySingleOrDefaultAsync<UserModel>(sql, user);
        }

        public async Task UpdateUserAvatarAsync(int userId, byte[] avatar)
        {
            const string sql = "UPDATE dbo.Users SET Avatar = @avatar WHERE Id = @userId;";
            await connection.ExecuteAsync(sql, new { userId, avatar });
        }

        public async Task<byte[]> GetUserAvatarAsync(int userId)
        {
            const string sql = "SELCT Avatar FROM dbo.Users WHERE Id = @userId;";
            return await connection.QuerySingleOrDefaultAsync<byte[]>(sql, new { userId });
        }

        public async Task UpdateUserAsync(UserModel user)
        {
            const string sql = "UPDATE dbo.Users SET Name = @Name, PayPalEmail = @PayPalEmail, PayPalCurrency = @PayPalCurrency " +
                "WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, user);
        }
    }
}
