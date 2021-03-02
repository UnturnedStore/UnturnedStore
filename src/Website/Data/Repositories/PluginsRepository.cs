using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Data.Repositories
{
    public class PluginsRepository
    {
        private readonly SqlConnection connection;

        public PluginsRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<bool> IsPluginOwnerAsync(int pluginId, int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Plugins v JOIN dbo.Branches b ON v.BranchId = b.Id " +
                "JOIN dbo.Products p ON p.Id = b.ProductId WHERE v.Id = @pluginId AND p.SellerId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { pluginId, userId });
        }

        public async Task TogglePluginAsync(int pluginId)
        {
            const string sql = "UPDATE dbo.Plugins SET IsEnabled = 1 - IsEnabled WHERE Id = @pluginId;";
            await connection.ExecuteAsync(sql, new { pluginId });
        }

        public async Task<PluginModel> AddPluginAsync(PluginModel plugin)
        {
            const string sql = "INSERT INTO dbo.Plugins (BranchId, Version, Changelog, FileName, Data, IsEnabled) " +
                "OUTPUT INSERTED.Id, INSERTED.BranchId, INSERTED.Version, INSERTED.Changelog, INSERTED.FileName, " +
                "INSERTED.IsEnabled, INSERTED.CreateDate " +
                "VALUES (@BranchId, @Version, @Changelog, @FileName, @Data, @IsEnabled);";

            var p = await connection.QuerySingleAsync<PluginModel>(sql, plugin);

            const string sql1 = "INSERT INTO dbo.PluginLibraries (PluginId, FileName, Data) " +
                "VALUES (@PluginId, @FileName, @Data);";
            foreach (var library in plugin.Libraries)
            {
                library.PluginId = p.Id;
                await connection.ExecuteAsync(sql1, library);
            }

            return p;
        }

        public async Task<PluginModel> GetPluginAsync(int pluginId, bool isSeller)
        {
            string sql = "SELECT p.*, b.Id, b.Name, p2.Id, p2.Name, l.* FROM dbo.Plugins p JOIN dbo.Branches b ON p.BranchId = b.Id " +
                "JOIN dbo.Products p2 ON p2.Id = b.ProductId LEFT JOIN dbo.PluginLibraries l ON l.PluginId = p.Id WHERE p.Id = @pluginId ";

            if (!isSeller)
            {
                sql += " AND p.IsEnabled = 1 AND b.IsEnabled = 1;";
            }

            PluginModel plugin = null;
            await connection.QueryAsync<PluginModel, BranchModel, ProductModel, PluginLibraryModel, PluginModel>(sql, (p, b, p2, l) => 
            {
                if (plugin == null)
                {
                    plugin = p;
                    plugin.Branch = b;
                    plugin.Branch.Product = p2;
                    plugin.Libraries = new List<PluginLibraryModel>();
                }
                
                if (l != null)
                {
                    plugin.Libraries.Add(l);
                }

                return null;
            }, new { pluginId });
            
            return plugin;
        }
    }
}
