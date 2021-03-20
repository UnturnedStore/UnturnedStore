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
    public class OrdersRepository
    {
        private readonly SqlConnection connection;

        public OrdersRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task UpdateOrderAsync(OrderModel order)
        {
            const string sql = "UPDATE dbo.Orders SET PaymentPayer = @PaymentPayer, Status = @Status, " +
                "TransactionId = @TransactionId, LastUpdate = SYSDATETIME() WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, order);
        }

        public async Task UpdateOrderPaymentUrlAsync(OrderModel order)
        {
            const string sql = "UPDATE dbo.Orders SET PaymentUrl = @PaymentUrl WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, order);
        }

        public async Task<OrderModel> AddOrderAsync(OrderModel order)
        {
            const string sql = "INSERT INTO dbo.Orders (BuyerId, SellerId, TotalPrice, Currency, PaymentMethod, PaymentReceiver, Status) " +
                "OUTPUT INSERTED.Id VALUES (@BuyerId, @SellerId, @TotalPrice, @Currency, @PaymentMethod, @PaymentReceiver, @Status);";

            order.Id = await connection.ExecuteScalarAsync<int>(sql, order);

            const string sql1 = "INSERT INTO dbo.OrderItems (OrderId, ProductId, ProductName, Price) " +
                "VALUES (@OrderId, @ProductId, @ProductName, @Price);";
            foreach (var item in order.Items)
            {
                item.OrderId = order.Id;
                await connection.ExecuteAsync(sql1, item);
            }

            return await GetOrderAsync(order.Id);
        }

        public async Task<IEnumerable<OrderModel>> GetOrdersAsync(int userId)
        {
            const string sql = "SELECT o.*, u.*, i.*, p.* FROM dbo.Orders o JOIN dbo.Users u ON o.SellerId = u.Id " +
                "LEFT JOIN dbo.OrderItems i ON o.Id = i.OrderId JOIN dbo.Products p ON i.ProductId = p.Id  WHERE o.BuyerId = @userId;";

            List<OrderModel> orders = new List<OrderModel>();
            await connection.QueryAsync<OrderModel, UserModel, OrderItemModel, ProductModel, OrderModel>(sql, (o, u, i, p) =>
            {
                var order = orders.FirstOrDefault(x => x.Id == o.Id);
                if (order == null)
                {
                    order = o;
                    order.Seller = u;
                    order.Items = new List<OrderItemModel>();
                    orders.Add(order);
                }

                if (i != null)
                {
                    i.Product = p;
                    order.Items.Add(i);
                }

                return null;
            }, new { userId });

            return orders;
        }

        public async Task<OrderModel> GetOrderAsync(int orderId)
        {
            const string sql = "SELECT o.*, u.*, i.*, p.* FROM dbo.Orders o JOIN dbo.Users u ON o.SellerId = u.Id " +
                "LEFT JOIN dbo.OrderItems i ON o.Id = i.OrderId JOIN dbo.Products p ON i.ProductId = p.Id WHERE o.Id = @orderId;";

            OrderModel order = null;
            await connection.QueryAsync<OrderModel, UserModel, OrderItemModel, ProductModel, OrderModel>(sql, (o, u, i, p) => 
            {
                if (order == null)
                {
                    order = o;
                    order.Seller = u;
                    order.Items = new List<OrderItemModel>();
                }

                if (i != null)
                {
                    i.Product = p;
                    order.Items.Add(i);
                }

                return null;
            }, new { orderId });

            return order;
        }
    }
}
