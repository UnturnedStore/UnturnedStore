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
            const string sql = "SELECT COUNT(1) FROM dbo.MessageReplies WHERE Id = @replyId and UserId = @userId;";
            return await connection.ExecuteScalarAsync<bool>(sql, new { replyId, userId });
        }

        public async Task<bool> IsMessageUserAsync(int messageId, int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Messages WHERE Id = @messageId AND (FromUserId = @userId OR ToUserId = @userId);";
            return await connection.ExecuteScalarAsync<bool>(sql, new { messageId, userId });
        }

        public async Task<MMessage> AddMessageAsync(MMessage message)
        {
            const string sql = "INSERT INTO dbo.Messages (FromUserId, ToUserId, Title) " +
                "OUTPUT INSERTED.Id, INSERTED.FromUserId, INSERTED.ToUserId, INSERTED.Title, INSERTED.CreateDate " +
                "VALUES (@FromUserId, @ToUserId, @Title);";

            var msg = await connection.QuerySingleAsync<MMessage>(sql, message);
            msg.Replies = new List<MMessageReply>();            

            foreach (var reply in message.Replies)
            {
                reply.MessageId = msg.Id;
                msg.Replies.Add(await AddMessageReplyAsync(reply));                
            }

            return msg;
        }

        public async Task<MMessageReply> AddMessageReplyAsync(MMessageReply reply)
        {
            const string sql = "INSERT INTO dbo.MessageReplies (MessageId, UserId, Content) " +
                "OUTPUT INSERTED.Id, INSERTED.MessageId, INSERTED.UserId, INSERTED.Content, INSERTED.LastUpdate, " +
                "INSERTED.CreateDate " +
                "VALUES (@MessageId, @UserId, @Content);";

            return await connection.QuerySingleAsync<MMessageReply>(sql, reply);
        }

        public async Task UpdateMessageReplyAsync(MMessageReply reply)
        {
            const string sql = "UPDATE dbo.MessageReplies SET Content = @Content, LastUpdate = SYSDATETIME() " +
                "WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, reply);
        }

        public async Task DeleteMessageReplyAsync(int replyId)
        {
            const string sql = "DELETE FROM dbo.MessageReplies WHERE Id = @replyId;";
            await connection.ExecuteAsync(sql, new { replyId });
        }

        public async Task CloseMessageAsync(int messageId, int userId)
        {
            const string sql = "UPDATE dbo.Messages SET IsClosed = 1, ClosingUserId = @userId WHERE Id = @messageId;";
            await connection.ExecuteAsync(sql, new { messageId, userId });
        }

        public async Task<MMessage> GetMessageAsync(int messageId)
        {
            const string sql = "SELECT m.*, fu.*, tu.* FROM dbo.Messages m " +
                "JOIN dbo.Users fu ON fu.Id = m.FromUserId " +
                "JOIN dbo.Users tu ON tu.Id = m.ToUserId " +
                "WHERE m.Id = @messageId";

            var msg = (await connection.QueryAsync<MMessage, MUser, MUser, MMessage>(sql, (m, fu, tu) =>
            {
                m.FromUser = fu;
                m.ToUser = tu;
                return m;
            }, new { messageId })).FirstOrDefault();

            const string sql1 = "SELECT * FROM dbo.MessageReplies WHERE MessageId = @messageId;";

            msg.Replies = (await connection.QueryAsync<MMessageReply>(sql1, new { messageId })).ToList();

            return msg;
        }

        public async Task<IEnumerable<MMessage>> GetMessagesAsync(int userId)
        {
            const string sql = "SELECT m.*, fu.*, tu.*, r.* FROM dbo.Messages m JOIN dbo.Users fu ON fu.Id = m.FromUserId JOIN dbo.Users tu ON tu.Id = m.ToUserId " +
                "LEFT JOIN dbo.MessageReplies r ON r.MessageId = m.Id WHERE m.FromUserId = @userId OR m.ToUserId = @userId;";

            var messages = new List<MMessage>();
            await connection.QueryAsync<MMessage, MUser, MUser, MMessageReply, MMessage>(sql, (m, fu, tu, r) => 
            {
                var msg = messages.FirstOrDefault(x => x.Id == m.Id);
                if (msg == null)
                {
                    msg = m;
                    m.FromUser = fu;
                    m.ToUser = tu;
                    msg.Replies = new List<MMessageReply>();
                    messages.Add(msg);
                }

                if (r != null)
                    msg.Replies.Add(r);
                
                return m;
            }, new { userId });

            return messages;
        }
    }
}
