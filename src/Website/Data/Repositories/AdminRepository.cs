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
    public class AdminRepository
    {
        private readonly SqlConnection connection;

        public AdminRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task UpdateUserAsync(UserModel user)
        {
            const string sql = "UPDATE dbo.Users SET Role = @Role WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, user);
        }
    }
}
