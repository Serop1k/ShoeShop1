using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Services
{
    public static class ProductService
    {
        public static List<Product> GetAll()
        {
            var list = new List<Product>();
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(@"
        SELECT p.product_id, p.article, p.name, p.category_id, c.category_name,
               p.description, p.manufacturer_id, m.manufacturer_name,
               p.supplier_id, s.supplier_name, p.unit_id, u.unit_name,
               p.price, p.discount, p.stock_qty, p.image_path
        FROM products p
        JOIN categories c    ON c.category_id     = p.category_id
        JOIN manufacturers m ON m.manufacturer_id = p.manufacturer_id
        JOIN suppliers s     ON s.supplier_id     = p.supplier_id
        JOIN units u         ON u.unit_id         = p.unit_id
        ORDER BY p.name", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read()) list.Add(Map(rdr));
            }
            return list;
        }

        public static void Add(Product p)
        {
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(@"
                INSERT INTO products
                (article,name,category_id,description,manufacturer_id,supplier_id,
                 unit_id,price,discount,stock_qty,image_path)
                VALUES (@a,@n,@c,@d,@m,@s,@u,@pr,@ds,@q,@img);
                SELECT LAST_INSERT_ID();", conn))
            {
                Bind(cmd, p);
                p.ProductId = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void Update(Product p)
        {
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(@"
                UPDATE products SET article=@a, name=@n, category_id=@c,
                    description=@d, manufacturer_id=@m, supplier_id=@s,
                    unit_id=@u, price=@pr, discount=@ds, stock_qty=@q,
                    image_path=@img
                WHERE product_id=@id", conn))
            {
                Bind(cmd, p);
                cmd.Parameters.AddWithValue("@id", p.ProductId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(
                "DELETE FROM products WHERE product_id=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public static bool IsInOrders(int productId)
        {
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM order_items WHERE product_id=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", productId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // -------- Справочники --------
        public static List<LookupItem> GetCategories()
            => GetLookup("categories", "category_id", "category_name");
        public static List<LookupItem> GetManufacturers()
            => GetLookup("manufacturers", "manufacturer_id", "manufacturer_name");
        public static List<LookupItem> GetSuppliers()
            => GetLookup("suppliers", "supplier_id", "supplier_name");
        public static List<LookupItem> GetUnits()
            => GetLookup("units", "unit_id", "unit_name");

        private static List<LookupItem> GetLookup(string table, string idCol, string nameCol)
        {
            var list = new List<LookupItem>();
            using (var conn = DatabaseContext.GetConnection())
            using (var cmd = new MySqlCommand(
                $"SELECT {idCol}, {nameCol} FROM {table} ORDER BY {nameCol}", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                    list.Add(new LookupItem { Id = rdr.GetInt32(0), Name = rdr.GetString(1) });
            }
            return list;
        }

        private static void Bind(MySqlCommand cmd, Product p)
        {
            cmd.Parameters.AddWithValue("@a", p.Article);
            cmd.Parameters.AddWithValue("@n", p.Name);
            cmd.Parameters.AddWithValue("@c", p.CategoryId);
            cmd.Parameters.AddWithValue("@d", (object)p.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@m", p.ManufacturerId);
            cmd.Parameters.AddWithValue("@s", p.SupplierId);
            cmd.Parameters.AddWithValue("@u", p.UnitId);
            cmd.Parameters.AddWithValue("@pr", p.Price);
            cmd.Parameters.AddWithValue("@ds", p.Discount);
            cmd.Parameters.AddWithValue("@q", p.StockQty);
            cmd.Parameters.AddWithValue("@img", (object)p.ImagePath ?? DBNull.Value);
        }

        private static Product Map(MySqlDataReader r) => new Product
        {
            ProductId = r.GetInt32(0),
            Article = r.GetString(1),
            Name = r.GetString(2),
            CategoryId = r.GetInt32(3),
            CategoryName = r.GetString(4),
            Description = r.IsDBNull(5) ? "" : r.GetString(5),
            ManufacturerId = r.GetInt32(6),
            ManufacturerName = r.GetString(7),
            SupplierId = r.GetInt32(8),
            SupplierName = r.GetString(9),
            UnitId = r.GetInt32(10),
            UnitName = r.GetString(11),
            Price = r.GetDecimal(12),
            Discount = r.GetInt32(13),
            StockQty = r.GetInt32(14),
            ImagePath = r.IsDBNull(15) ? null : r.GetString(15)
        };
    }
}