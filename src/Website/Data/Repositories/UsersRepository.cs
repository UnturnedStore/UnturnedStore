using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Website.Shared.Models.Database;
using Website.Shared.Models.Children;
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

        public async Task<UserProfile> GetUserProfileAsync(int userId)
        {
            const string sql = "dbo.GetUserProfile";

            UserProfile user = null;

            await connection.QueryAsync<UserProfile, MProduct, MProductSale, UserProfile>(sql, (u, p, ps) => 
            { 
                if (user == null)
                {
                    user = u;
                    user.Products = new List<MProduct>();
                }

                if (p != null)
                {
                    p.Sale = ps;
                    user.Products.Add(p);
                }

                return null;
            }, new { UserId = userId }, commandType: CommandType.StoredProcedure);

            return user;
        }

        public async Task<int> GetUserAvatarImageIdAsync(int userId)
        {
            const string sql = "SELECT AvatarImageId FROM dbo.Users WHERE Id = @userId;";
            return await connection.ExecuteScalarAsync<int>(sql, new { userId });
        }

        public async Task<MUser> GetUserAsync(int userId)
        {
            const string sql = "SELECT * FROM dbo.Users WHERE Id = @userId;";
            return await connection.QuerySingleOrDefaultAsync<MUser>(sql, new { userId });
        }

        public async Task<T> GetUserAsync<T>(int userId) where T : UserInfo
        {
            const string sql = "SELECT * FROM dbo.Users WHERE Id = @userId;";
            return await connection.QuerySingleOrDefaultAsync<T>(sql, new { userId });
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
            const string sql = "UPDATE dbo.Users SET Name = @Name, AvatarImageId = @AvatarImageId, Color = @Color, " +
                "Biography = @Biography WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, user);
        }

        public async Task UpdateSellerAsync(MUser user)
        {
            const string sql = "UPDATE dbo.Users SET IsPayPalEnabled = @IsPayPalEnabled, PayPalAddress = @PayPalAddress, " +
                "IsNanoEnabled = @IsNanoEnabled, NanoAddress = @NanoAddress, TermsAndConditions = @TermsAndConditions " + 
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
            const string sql = "SELECT Id, Name, SteamId, Role, AvatarImageId, CreateDate FROM dbo.Users;";
            return await connection.QueryAsync<MUser>(sql);
        }
    }
}
