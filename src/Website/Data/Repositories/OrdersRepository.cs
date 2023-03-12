using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models;
using Website.Shared.Models.Database;

namespace Website.Data.Repositories
{
    public class OrdersRepository
    {
        private readonly SqlConnection connection;

        public OrdersRepository(SqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task UpdateOrderAsync(MOrder order)
        {
            const string sql = "UPDATE dbo.Orders SET Status = @Status, LastUpdate = @LastUpdate WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, order);
        }

        public async Task<MOrder> AddOrderAsync(MOrder order)
        {
            const string sql = "INSERT INTO dbo.Orders (PaymentId, BuyerId, SellerId, TotalPrice, Currency, PaymentMethod, PaymentReceiver, Status) " +
                "OUTPUT INSERTED.Id VALUES (@PaymentId, @BuyerId, @SellerId, @TotalPrice, @Currency, @PaymentMethod, @PaymentReceiver, @Status);";

            order.Id = await connection.ExecuteScalarAsync<int>(sql, order);

            const string sql1 = "INSERT INTO dbo.OrderItems (OrderId, ProductId, SaleId, CouponId, ProductName, Price, CouponMultiplier) " +
                "VALUES (@OrderId, @ProductId, @SaleId, @CouponId, @ProductName, @Price, @CouponMultiplier);";
            foreach (var item in order.Items)
            {
                item.OrderId = order.Id;
                await connection.ExecuteAsync(sql1, item);
            }

            return await GetOrderAsync(order.Id);
        }

        public async Task<IEnumerable<MOrder>> GetOrdersAsync(int userId)
        {
            const string sql = "SELECT o.*, u.*, i.*, p.*, ps.*, co.Id, co.ProductId, co.CouponName, co.IsEnabled FROM dbo.Orders o JOIN dbo.Users u ON o.SellerId = u.Id " +
                "LEFT JOIN dbo.OrderItems i ON o.Id = i.OrderId JOIN dbo.Products p ON i.ProductId = p.Id LEFT JOIN dbo.ProductSales ps ON i.SaleId = ps.Id LEFT JOIN dbo.ProductCoupons co ON i.CouponId = co.Id WHERE o.BuyerId = @userId;";

            List<MOrder> orders = new List<MOrder>();
            await connection.QueryAsync<MOrder, Seller, MOrderItem, MProduct, MProductSale, MProductCoupon, MOrder>(sql, (o, u, i, p, ps, co) =>
            {
                var order = orders.FirstOrDefault(x => x.Id == o.Id);
                if (order == null)
                {
                    order = o;
                    order.Seller = u;
                    order.Items = new List<MOrderItem>();
                    orders.Add(order);
                }

                if (i != null)
                {
                    i.Product = p;
                    i.Product.Sale = ps;
                    i.Coupon = co;
                    order.Items.Add(i);
                }

                return null;
            }, new { userId });

            return orders;
        }

        public async Task<MOrder> GetOrderAsync(Guid paymentId)
        {
            const string sql = "SELECT o.*, u.*, u2.*, i.*, p.*, ps.*, co.Id, co.ProductId, co.CouponName, co.IsEnabled " +
                "FROM dbo.Orders o " +
                "JOIN dbo.Users u ON o.SellerId = u.Id " +
                "JOIN dbo.Users u2 ON o.BuyerId = u2.Id " +
                "LEFT JOIN dbo.OrderItems i ON o.Id = i.OrderId " +
                "JOIN dbo.Products p ON i.ProductId = p.Id " +
                "LEFT JOIN dbo.ProductSales ps ON i.SaleId = ps.Id " +
                "LEFT JOIN dbo.ProductCoupons co ON i.CouponId = co.Id " +
                "WHERE o.PaymentId = @paymentId;";

            return await GetOrderSharedAsync(sql, new { paymentId });
        }

        public async Task<MOrder> GetOrderAsync(int orderId)
        {
            const string sql = "SELECT o.*, u.*, u2.*, i.*, p.*, ps.*, co.Id, co.ProductId, co.CouponName, co.IsEnabled " +
                "FROM dbo.Orders o " +
                "JOIN dbo.Users u ON o.SellerId = u.Id " +
                "JOIN dbo.Users u2 ON o.BuyerId = u2.Id " + 
                "LEFT JOIN dbo.OrderItems i ON o.Id = i.OrderId " +
                "JOIN dbo.Products p ON i.ProductId = p.Id " +
                "LEFT JOIN dbo.ProductSales ps ON i.SaleId = ps.Id " +
                "LEFT JOIN dbo.ProductCoupons co ON i.CouponId = co.Id " +
                "WHERE o.Id = @orderId;";

            return await GetOrderSharedAsync(sql, new { orderId });
        }

        private async Task<MOrder> GetOrderSharedAsync(string sql, object param)
        {
            MOrder order = null;
            await connection.QueryAsync<MOrder, Seller, UserInfo, MOrderItem, MProduct, MProductSale, MProductCoupon, MOrder>(sql, (o, u, u2, i, p, ps, co) =>
            {
                if (order == null)
                {
                    order = o;
                    order.Seller = u;
                    order.Buyer = u2;
                    order.Items = new List<MOrderItem>();
                }

                if (i != null)
                {
                    i.Product = p;
                    i.Product.Sale = ps;
                    i.Coupon = co;
                    order.Items.Add(i);
                }

                return null;
            }, param);

            return order;
        }
    }
}
