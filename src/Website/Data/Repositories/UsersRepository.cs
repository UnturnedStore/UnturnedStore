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

        public async Task<MUser> GetUserProfileAsync(int userId)
        {
            const string sql = "SELECT u.Id, u.Name, u.Role, u.SteamId, u.AvatarImageId, u.Color, u.CreateDate, p.* " +
                "FROM dbo.Users u " +
                "LEFT JOIN dbo.Products p ON u.Id = p.SellerId AND p.IsEnabled = 1 " +
                "WHERE u.Id = @userId;";

            MUser user = null;
            await connection.QueryAsync<MUser, MProduct, MUser>(sql, (u, p) =>
            {
                if (user == null)
                {
                    user = u;
                    user.Products = new List<MProduct>();
                }

                if (p != null)
                    user.Products.Add(p);

                return null;
            }, new { userId });
            
            return user;
        }

        public async Task<MUser> GetUserPublicAsync(int userId)
        {
            const string sql = "SELECT u.Id, u.Name, u.Role, u.SteamId, u.TermsAndConditions, u.CreateDate, c.*, p.* FROM dbo.Users u LEFT JOIN dbo.ProductCustomers c ON u.Id = c.UserId " +
                "LEFT JOIN dbo.Products p ON c.ProductId = p.Id WHERE u.Id = @userId;";

            MUser user = null;

            await connection.QueryAsync<MUser, MProductCustomer, MProduct, MUser>(sql, (u, c, p) => 
            { 
                if (user == null)
                {
                    user = u;
                    user.Customers = new List<MProductCustomer>();
                }

                if (c != null)
                {
                    c.Product = p;
                    user.Customers.Add(c);
                }

                return null;
            }, new { userId });

            return user;
        }

        public async Task<MUser> GetUserPrivateAsync(int userId)
        {
            const string sql = "SELECT * FROM dbo.Users WHERE Id = @userId;";
            return await connection.QuerySingleOrDefaultAsync<MUser>(sql, new { userId });
        }
        
        public async Task<MUser> GetUserAsync(string steamId)
        {
            const string sql = "SELECT Id, Name, Role, SteamId, CreateDate " +
                "FROM dbo.Users WHERE SteamId = @steamId;";

            return await connection.QuerySingleOrDefaultAsync<MUser>(sql, new { steamId });
        }

        public async Task<MUser> AddUserAsync(MUser user)
        {
            const string sql = "INSERT INTO dbo.Users (Name, Role, SteamId, AvatarImageId) " +
                "OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Role, INSERTED.SteamId, INSERTED.AvatarImageId, INSERTED.CreateDate " +
                "VALUES (@Name, @Role, @SteamId, @AvatarImageId);";

            return await connection.QuerySingleOrDefaultAsync<MUser>(sql, user);
        }

        public async Task UpdateProfileAsync(MUser user)
        {
            const string sql = "UPDATE dbo.Users SET Name = @Name, AvatarImageId = @AvatarImageId, Color = @Color WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, user);
        }

        public async Task UpdateSellerAsync(MUser user)
        {
            const string sql = "UPDATE dbo.Users SET PayPalEmail = @PayPalEmail, TermsAndConditions = @TermsAndConditions " + 
                "WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, user);
        }

        public async Task UpdateNotificationsAsync(MUser user)
        {
            const string sql = "UPDATE dbo.Users SET DiscordWebhookUrl = @DiscordWebhookUrl WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, user);
        }

        public async Task<string> GetUserDiscordWebhookUrl(int userId)
        {
            const string sql = "SELECT DiscordWebhookUrl FROM dbo.Users WHERE Id = @userId;";
            return await connection.ExecuteScalarAsync<string>(sql, new { userId });
        }

        public async Task<IEnumerable<MUser>> GetUsersAsync()
        {
            const string sql = "SELECT Id, Name, SteamId, Role, CreateDate FROM dbo.Users;";
            return await connection.QueryAsync<MUser>(sql);
        }
    }
}
