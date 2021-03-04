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
    public class MessagesRepository
    {
        private readonly SqlConnection connection;

        public MessagesRepository(SqlConnection connection)
        {
            this.connection = connection;
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

        public async Task<MessageModel> GetMessageAsync()
        {
            const string sql = "SELECT * FROM dbo.Messages m LEFT JOIN dbo.MessageReplies r ON m.Id = r.MessageId WHERE Id = @Id;";

            MessageModel message = null;

            await connection.QueryAsync<MessageModel, MessageReplyModel, MessageModel>(sql, (m, r) => 
            { 
                if (message == null)
                {
                    message = m;
                    message.Replies = new List<MessageReplyModel>();
                }

                if (r != null)
                {
                    message.Replies.Add(r);
                }

                return null;
            });

            const string sql1 = "SELECT * FROM dbo.Users WHERE Id = @Id;";

            message.FromUser = await connection.QuerySingleOrDefaultAsync<UserModel>(sql1, new { Id = message.FromUserId });
            message.ToUser = await connection.QuerySingleOrDefaultAsync<UserModel>(sql1, new { Id = message.ToUserId });

            return message;
        }
    }
}
