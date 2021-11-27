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
    public class NanoRepository
    {
        private readonly SqlConnection connection;

        public NanoRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task UpdatePaymentAsync(MNanoPayment payment)
        {
            const string sql = "UPDATE dbo.NanoPayments " +
                "SET SendAddress = @SendAddress, ReceiveDate = SYSDATETIME(), Amount = @Amount " +
                "WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, payment);
        }

        public async Task<IEnumerable<MNanoPayment>> GetPendingPaymentsAsync()
        {
            const string sql = "SELECT * FROM dbo.NanoPayments WHERE IsReceived = 0;";
            return await connection.QueryAsync<MNanoPayment>(sql);
        }

        public async Task<MNanoPayment> AddPaymentAsync(MNanoPayment payment)
        {
            const string sql = "INSERT INTO dbo.NanoPayments (OrderId, SellerAddress, ReceiveAddress, ReceivePrivateKey, Amount) " +
                "OUTPUT INSERTED.Id, INSERTED.OrderId, INSERTED.SellerAddress, INSERTED.ReceiveAddress, INSERTED.ReceivePrivateKey, " +
                "INSERTED.SendAddress, INSERTED.Amount, INSERTED.ReceiveDate, INSERTED.CreateDate " +
                "VALUES (@OrderId, @SellerAddress, @ReceiveAddress, @ReceivePrivateKey, @Amount);";

            return await connection.QuerySingleOrDefaultAsync<MNanoPayment>(sql, payment);
        }
    }
}
