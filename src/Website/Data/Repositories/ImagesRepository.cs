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
    public class ImagesRepository
    {
        private readonly SqlConnection connection;

        public ImagesRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<ImageModel> GetImageAsync(int id)
        {
            const string sql = "SELECT * FROM dbo.Images WHERE Id = @id;";
            return await connection.QuerySingleOrDefaultAsync<ImageModel>(sql, new { id });
        }

        public async Task<int> AddImageAsync(ImageModel image)
        {
            const string sql = "INSERT INTO dbo.Images (Name, ContentType, Content) " +
                "OUTPUT INSERTED.Id " +
                "VALUES (@Name, @ContentType, @Content);";

            return await connection.ExecuteScalarAsync<int>(sql, image);
        }
    }
}
