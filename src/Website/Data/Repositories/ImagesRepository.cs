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
    public class ImagesRepository
    {
        private readonly SqlConnection connection;

        public ImagesRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<MImage> GetImageAsync(int id)
        {
            const string sql = "SELECT * FROM dbo.Images WHERE Id = @id;";
            return await connection.QuerySingleOrDefaultAsync<MImage>(sql, new { id });
        }

        public async Task<int> AddImageAsync(MImage image)
        {
            const string sql = "INSERT INTO dbo.Images (Name, ContentType, Content, UserId) " +
                "OUTPUT INSERTED.Id " +
                "VALUES (@Name, @ContentType, @Content, @UserId);";

            return await connection.ExecuteScalarAsync<int>(sql, image);
        }
    }
}
