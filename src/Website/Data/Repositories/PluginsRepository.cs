using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models.Database;
using Website.Shared.Params;
using Website.Shared.Results;

namespace Website.Data.Repositories
{
    public class PluginsRepository
    {
        private readonly SqlConnection connection;

        public PluginsRepository(SqlConnection connection)
        {
            this.connection = connection;
        }


        public async Task<GetPluginResult> GetPluginAsync(GetPluginParams @params)
        {
            const string sql = "dbo.GetPlugin";

            DynamicParameters p = new();
            p.Add(name: "@LicenseKey", value: @params.LicenseKey, dbType: DbType.Guid, direction: ParameterDirection.Input);
            p.Add(name: "@ProductName", value: @params.ProductName, dbType: DbType.String, size: 255, direction: ParameterDirection.Input);
            p.Add(name: "@BranchName", value: @params.BranchName, dbType: DbType.String, size: 255, direction: ParameterDirection.Input);
            p.Add(name: "@VersionName", value: @params.VersionName, dbType: DbType.String, size: 255, direction: ParameterDirection.Input);
            p.Add(name: "@ServerName", value: @params.ServerInfo.Name, dbType: DbType.String, size: 255, direction: ParameterDirection.Input);
            p.Add(name: "@Host", value: @params.ServerInfo.Host, dbType: DbType.String, size: 255, direction: ParameterDirection.Input);
            p.Add(name: "@Port", value: @params.ServerInfo.Port, dbType: DbType.Int32, direction: ParameterDirection.Input);
            p.Add(name: "@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
            p.Add(name: "@RetVal", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            GetPluginResult result = new();

            result.Version = await connection.QuerySingleOrDefaultAsync<MVersion>(sql, p, commandType: CommandType.StoredProcedure);
            result.ErrorMessage = p.Get<string>("@ErrorMessage");
            result.ReturnCode = p.Get<int>("@RetVal");
            return result;
        } 
    }
}
