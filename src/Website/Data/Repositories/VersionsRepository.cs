using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Website.Shared.Enums;
using Website.Shared.Models.Database;

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

        public async Task<bool> IsVersionSellerAsync(int versionId, int userId)
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

        public async Task<MVersion> AddVersionAsync(MVersion version)
        {
            const string sql = "INSERT INTO dbo.Versions (BranchId, Name, Changelog, FileName, ContentType, Content, IsEnabled) " +
                "OUTPUT INSERTED.Id, INSERTED.BranchId, INSERTED.Name, INSERTED.Changelog, INSERTED.ContentType, INSERTED.FileName, " +
                "INSERTED.DownloadsCount, INSERTED.IsEnabled, INSERTED.CreateDate " +
                "VALUES (@BranchId, @Name, @Changelog, @FileName, @ContentType, @Content, @IsEnabled);";

            var p = await connection.QuerySingleAsync<MVersion>(sql, version);
            return p;
        }

        public async Task UpdateVersionAsync(MVersion version)
        {
            const string sql = "UPDATE dbo.Versions SET Name = @Name, Changelog = @Changelog WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, version);
        }

        public async Task<MVersion> GetVersionAsync(int versionId, bool isSeller)
        {
            string sql = "SELECT v.*, b.*, p.* FROM dbo.Versions v JOIN dbo.Branches b ON v.BranchId = b.Id " +
                "JOIN dbo.Products p ON p.Id = b.ProductId WHERE v.Id = @versionId ";

            if (!isSeller)
            {
                sql += " AND v.IsEnabled = 1 AND b.IsEnabled = 1;";
            }

            MVersion version = null;
            await connection.QueryAsync<MVersion, MBranch, MProduct, MVersion>(sql, (v, b, p) => 
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

        public async Task<MVersion> GetLatestVersionAsync(int productId)
        {
            string sql = "SELECT TOP 1 v.*, b.*, p.* " + 
                "FROM dbo.Versions v " + 
                "JOIN dbo.Branches b ON v.BranchId = b.Id " + 
                "JOIN dbo.Products p ON p.Id = b.ProductId " + 
                "WHERE p.Id = @productId AND b.IsEnabled = 1 AND v.IsEnabled = 1 " +
                "ORDER BY b.CreateDate ASC, v.CreateDate DESC;";

            return await QueryVersionAsync(sql, new { productId });
        }

        private async Task<MVersion> QueryVersionAsync(string sql, object param)
        {
            MVersion version = null;
            await connection.QueryAsync<MVersion, MBranch, MProduct, MVersion>(sql, (v, b, p) =>
            {
                if (version == null)
                {
                    version = v;
                    version.Branch = b;
                    version.Branch.Product = p;
                }

                return null;
            }, param);

            return version;
        }

        public async Task IncrementDownloadsCount(int versionId)
        {
            const string sql = "UPDATE dbo.Versions SET DownloadsCount = DownloadsCount + 1 WHERE Id = @versionId;";
            await connection.ExecuteAsync(sql, new { versionId });
        }
    }
}