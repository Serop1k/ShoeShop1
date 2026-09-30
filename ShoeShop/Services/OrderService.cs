using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Services
{
    public static class OrderService
    {
        public static List<Order> GetAll()
        {
            var list = new List<Order>();
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(@"
                SELECT o.order_id, o.article, o.status_id, s.status_name,
                       o.pickup_point, o.order_date, o.delivery_date
                FROM orders o
                JOIN order_statuses s ON s.status_id = o.status_id
                ORDER BY o.order_date DESC", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    list.Add(new Order
                    {
                        OrderId = rdr.GetInt32(0),
                        Article = rdr.GetString(1),
                        StatusId = rdr.GetInt32(2),
                        StatusName = rdr.GetString(3),
                        PickupPoint = rdr.GetString(4),
                        OrderDate = rdr.GetDateTime(5),
                        DeliveryDate = rdr.IsDBNull(6) ? (DateTime?)null : rdr.GetDateTime(6)
                    });
                }
            }
            return list;
        }

        public static void Add(Order o)
        {
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(@"
                INSERT INTO orders (article, status_id, pickup_point, order_date, delivery_date)
                VALUES (@a, @s, @p, @od, @dd);
                SELECT LAST_INSERT_ID();", conn))
            {
                Bind(cmd, o);
                o.OrderId = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void Update(Order o)
        {
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(@"
                UPDATE orders SET article=@a, status_id=@s, pickup_point=@p,
                                  order_date=@od, delivery_date=@dd
                WHERE order_id=@id", conn))
            {
                Bind(cmd, o);
                cmd.Parameters.AddWithValue("@id", o.OrderId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int orderId)
        {
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(
                "DELETE FROM orders WHERE order_id=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", orderId);
                cmd.ExecuteNonQuery();
            }
        }

        public static List<LookupItem> GetStatuses()
        {
            var list = new List<LookupItem>();
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(
                "SELECT status_id, status_name FROM order_statuses ORDER BY status_name", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                    list.Add(new LookupItem { Id = rdr.GetInt32(0), Name = rdr.GetString(1) });
            }
            return list;
        }

        private static void Bind(MySqlCommand cmd, Order o)
        {
            cmd.Parameters.AddWithValue("@a", o.Article);
            cmd.Parameters.AddWithValue("@s", o.StatusId);
            cmd.Parameters.AddWithValue("@p", o.PickupPoint);
            cmd.Parameters.AddWithValue("@od", o.OrderDate);
            cmd.Parameters.AddWithValue("@dd", (object)o.DeliveryDate ?? DBNull.Value);
        }
    }
}