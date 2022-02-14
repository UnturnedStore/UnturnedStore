using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models;
using Website.Shared.Models.Database;
using Website.Shared.Results;

namespace Website.Data.Repositories
{
    public class HomeRepository
    {
        private readonly SqlConnection connection;

        public HomeRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<HomeStatisticsResult> GetHomeStatisticsAsync()
        {
            const string sql = "dbo.GetHomeStatistics";
            return await connection.QuerySingleOrDefaultAsync<HomeStatisticsResult>(sql, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<MProduct>> GetPromotedProductsAsync(int userId)
        {
            const string sql = "dbo.GetPromotedProducts";
            return await connection.QueryAsync<MProduct, Seller, MProduct>(sql, (p, u) =>
            {
                p.Seller = u;
                return p;
            }, new { UserId = userId }, commandType: CommandType.StoredProcedure);
        }
    }
}
