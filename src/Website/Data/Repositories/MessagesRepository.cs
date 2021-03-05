using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Data.Repositories
{
    public class MessagesRepository
    {
        private readonly SqlConnection connection;

        public MessagesRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<bool> IsMessageReplyUserAsync(int replyId, int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.MessageReplies r JOIN dbo.Messages m ON r.MessageId = m.Id " +
                "WHERE r.Id = @replyId AND (m.FromUserId = @userId OR m.ToUserId = @userId)";
            return await connection.ExecuteScalarAsync<bool>(sql, new { replyId, userId });
        }

        public async Task<bool> IsMessageUserAsync(int messageId, int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Messages WHERE Id = @messageId AND (FromUserId = @userId OR ToUserId = @userId);";
            return await connection.ExecuteScalarAsync<bool>(sql, new { messageId, userId });
        }

        public async Task<MessageModel> AddMessageAsync(MessageModel message)
        {
            const string sql = "INSERT INTO dbo.Messages (FromUserId, ToUserId, Title) " +
                "OUTPUT INSERTED.Id, INSERTED.FromUserId, INSERTED.ToUserId, INSERTED.Title, INSERTED.CreateDate " +
                "VALUES (@FromUserId, @ToUserId, @Title);";

            var msg = await connection.QuerySingleAsync<MessageModel>(sql, message);
            msg.Replies = new List<MessageReplyModel>();            

            foreach (var reply in message.Replies)
            {
                reply.MessageId = msg.Id;
                msg.Replies.Add(await AddMessageReplyAsync(reply));                
            }

            return msg;
        }

        public async Task<MessageReplyModel> AddMessageReplyAsync(MessageReplyModel reply)
        {
            const string sql = "INSERT INTO dbo.MessageReplies (MessageId, UserId, Content) " +
                "OUTPUT INSERTED.Id, INSERTED.MessageId, INSERTED.UserId, INSERTED.Content, INSERTED.LastUpdate, " +
                "INSERTED.CreateDate " +
                "VALUES (@MessageId, @UserId, @Content);";

            return await connection.QuerySingleAsync<MessageReplyModel>(sql, reply);
        }

        public async Task UpdateMessageReplyAsync(MessageReplyModel reply)
        {
            const string sql = "UPDATE dbo.MessageReplies SET Content = @Content, LastUpdate = SYSDATETIME() " +
                "WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, reply);
        }

        public async Task<MessageModel> GetMessageAsync(int messageId)
        {
            const string sql = "SELECT m.*, fu.*, tu.* FROM dbo.Messages " +
                "JOIN dbo.Users fu ON fu.Id = m.FromUserId " +
                "JOIN dbo.Users tu ON tu.Id = m.ToUserId " +
                "WHERE Id = @messageId";

            var msg = (await connection.QueryAsync<MessageModel, UserModel, UserModel, MessageModel>(sql, (m, fu, tu) =>
            {
                m.FromUser = fu;
                m.ToUser = tu;
                return m;
            })).FirstOrDefault();

            const string sql1 = "SELECT * FROM dbo.MessageReplies WHERE MessageId = @messageId;";

            msg.Replies = (await connection.QueryAsync<MessageReplyModel>(sql1, new { messageId })).ToList();

            return msg;
        }

        public async Task<IEnumerable<MessageModel>> GetMessagesAsync(int userId)
        {
            const string sql = "SELECT m.*, fu.*, tu.* FROM dbo.Messages JOIN dbo.Users fu ON fu.Id = m.FromUserId JOIN dbo.Users tu ON tu.Id = m.ToUserId " +
                "WHERE m.FromUserId = @userId OR m.ToUserId = @userId;";

            return await connection.QueryAsync<MessageModel, UserModel, UserModel, MessageModel>(sql, (m, fu, tu) => 
            {
                m.FromUser = fu;
                m.ToUser = tu;
                return m;
            });
        }
    }
}
