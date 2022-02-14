using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<StatisticsResult> GetStatisticsAsync()
        {
            const string sql = "dbo.GetHomeStatistics";
            return await connection.QuerySingleOrDefaultAsync<StatisticsResult>(sql, commandType: CommandType.StoredProcedure);
        }
    }
}
