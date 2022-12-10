using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Enums;
using Website.Shared.Models.Database;

namespace Website.Data.Repositories
{
    public class BranchesRepository
    {
        private readonly SqlConnection connection;

        public BranchesRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<bool> IsBranchSellerAsync(int branchId, int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Branches b JOIN dbo.Products p ON p.Id = b.ProductId " +
                "WHERE b.Id = @branchId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { branchId, userId });
        }

        public async Task<bool> IsBranchEnabled(int branchId)
        {
            const string sql = "SELECT IsEnabled FROM dbo.Branches WHERE Id = @branchId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { branchId });
        }

        public async Task<ProductStatus> GetProductStatusAsync(int branchId)
        {
            const string sql = "SELECT p.Status FROM dbo.Products p JOIN dbo.Branches b ON b.Id = @branchId WHERE p.Id = b.ProductId;";
            return (ProductStatus)await connection.ExecuteScalarAsync<int>(sql, new { branchId });
        }

        public async Task<MBranch> AddBranchAsync(MBranch branch)
        {
            const string sql = "INSERT INTO dbo.Branches (Name, Description, ProductId, IsEnabled) " +
                "OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Description, INSERTED.IsEnabled, INSERTED.CreateDate " +
                "VALUES (@Name, @Description, @ProductId, @IsEnabled);";
            branch = await connection.QuerySingleAsync<MBranch>(sql, branch);
            branch.Versions = new List<MVersion>();
            return branch;
        }

        public async Task UpdateBranchAsync(MBranch branch)
        {
            const string sql = "UPDATE dbo.Branches SET Name = @Name, Description = @Description, " +
                "IsEnabled = @IsEnabled WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, branch);
        }

        public async Task<MBranch> GetBranchAsync(int branchId)
        {
            const string sql = "SELECT * FROM dbo.Branches WHERE Id = @branchId;";

            return await connection.QuerySingleOrDefaultAsync<MBranch>(sql, new { branchId });
        }
    }
}
