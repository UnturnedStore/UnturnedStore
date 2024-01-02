using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models.Database;

namespace Website.Data.Repositories
{
    public class CustomersRepository
    {
        private readonly SqlConnection connection;

        public CustomersRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<IEnumerable<MCustomerServer>> GetCustomerServersByCustomerIdAsync(int customerId)
        {
            const string sql = "SELECT * FROM CustomerServers WHERE CustomerId = @customerId";

            return await connection.QueryAsync<MCustomerServer>(sql, new { customerId });
        }
    }
}
