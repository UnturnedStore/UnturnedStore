using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Data.Repositories
{
    public class VersionsRepository
    {
        private readonly SqlConnection connection;

        public VersionsRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<bool> IsVersionCustomerAsync(int versionId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.Versions v JOIN dbo.Branches b ON v.BranchId = b.Id JOIN dbo.Products p ON p.Id = b.ProductId " +
                "JOIN dbo.ProductCustomers c ON b.ProductId = c.ProductId WHERE v.Id = @versionId AND (c.UserId = @userId OR p.SellerId = @userId);";
            return await connection.ExecuteScalarAsync<bool>(sql, new { versionId, userId });
        }

        public async Task<bool> IsVersionOwnerAsync(int versionId, int userId)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.Versions v JOIN dbo.Branches b ON v.BranchId = b.Id " +
                "JOIN dbo.Products p ON p.Id = b.ProductId WHERE v.Id = @versionId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { versionId, userId });
        }

        public async Task ToggleVersionAsync(int versionId)
        {
            const string sql = "UPDATE dbo.Versions SET IsEnabled = 1 - IsEnabled WHERE Id = @versionId;";
            await connection.ExecuteAsync(sql, new { versionId });
        }

        public async Task<VersionModel> AddVersionAsync(VersionModel version)
        {
            const string sql = "INSERT INTO dbo.Versions (BranchId, Name, Changelog, FileName, ContentType, Content, IsEnabled) " +
                "OUTPUT INSERTED.Id, INSERTED.BranchId, INSERTED.Name, INSERTED.Changelog, INSERTED.ContentType, INSERTED.FileName, " +
                "INSERTED.DownloadsCount, INSERTED.IsEnabled, INSERTED.CreateDate " +
                "VALUES (@BranchId, @Name, @Changelog, @FileName, @ContentType, @Content, @IsEnabled);";

            var p = await connection.QuerySingleAsync<VersionModel>(sql, version);
            return p;
        }

        public async Task<VersionModel> GetVersionAsync(int versionId, bool isSeller)
        {
            string sql = "SELECT v.*, b.Id, b.Name, p2.Id, p2.Name, p2.Price, l.* FROM dbo.Versions v JOIN dbo.Branches b ON v.BranchId = b.Id " +
                "JOIN dbo.Products p ON p.Id = b.ProductId WHERE v.Id = @versionId ";

            if (!isSeller)
            {
                sql += " AND v.IsEnabled = 1 AND b.IsEnabled = 1;";
            }

            VersionModel version = null;
            await connection.QueryAsync<VersionModel, BranchModel, ProductModel, VersionModel>(sql, (v, b, p) => 
            {
                if (version == null)
                {
                    version = v;
                    version.Branch = b;
                    version.Branch.Product = p;
                }

                return null;
            }, new { versionId });
            
            return version;
        }

        public async Task IncrementDownloadsCount(int versionId)
        {
            const string sql = "UPDATE dbo.Versions SET DownloadsCount = DownloadsCount + 1 WHERE Id = @versionId;";
            await connection.ExecuteAsync(sql, new { versionId });
        }
    }
}
