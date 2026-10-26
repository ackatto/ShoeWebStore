using Microsoft.Data.Sqlite;
using ShoeWebStore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
namespace ShoeWebStore.Data
{
    public static class DatabaseHelper
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "store.db");
        public static string cStr => $"Data Source={DbPath}";

        public static void InitializeDatabase()
        {
            using var conn = new SqliteConnection(cStr);
            conn.Open();

            string sql = @"CREATE TABLE IF NOT EXISTS Users(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Login TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                FullName TEXT NOT NULL,
                Role TEXT NOT NULL CHECK(Role IN('Гость','Клиент','Менеджер','Администратор'))
                );
                
                CREATE TABLE IF NOT EXISTS Categories(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Manufacturers(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Suppliers(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Products(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                CategoryId INTEGER REFERENCES Categories(Id),
                ManufacturerId INTEGER REFERENCES Manufacturers(Id),
                SupplierId INTEGER REFERENCES Suppliers(Id),
                Price DECIMAL(10,2) NOT NULL,
                Unit TEXT NOT NULL,
                StockQuantity INTEGER NOT NULL DEFAULT 0,
                Discount DECIMAL(5,2) DEFAULT 0,
                ImagePath TEXT
                );

               CREATE TABLE IF NOT EXISTS Orders(
               Id INTEGER PRIMARY KEY AUTOINCREMENT,
               OrderDate TEXT,
               Status TEXT,
               UserId INTEGER REFERENCES Users(Id)
               );

                CREATE TABLE IF NOT EXISTS OrderItems(
                OrderId INTEGER REFERENCES Orders(Id),
                ProductId INTEGER REFERENCES Products(Id),
                Quantity INTEGER,
                PRIMARY KEY (OrderId,ProductId)
                );";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();
            /* ↓ Заполнение пользователей, категорий, производителей и поставщиков ↓ */

            using (var check = new SqliteCommand("SELECT COUNT(*) FROM Users", conn))
            {
                if ((long)check.ExecuteScalar() == 0)
                {
                    string insert = @"INSERT INTO Users(Login,Password,FullName,Role) VALUES 
                        ('admin','admin','Сис.Админ','Администратор'),
                        ('manager','manager','Менеджер Иванов А.А','Менеджер'),
                        ('client','client','Клиент Лашков А.А.','Клиент');";
                    using var insertCmd = new SqliteCommand(insert, conn);
                    insertCmd.ExecuteNonQuery();
                }
            }

            using (var check = new SqliteCommand("SELECT COUNT(*) FROM Categories", conn))
            {
                if ((long)check.ExecuteScalar() == 0)
                {
                    string insert = "INSERT INTO Categories(Name) VALUES ('Кроссовки'),('Ботинки'),('Сапоги'),('Кеды');";
                    using var insertCmd = new SqliteCommand(insert, conn);
                    insertCmd.ExecuteNonQuery();
                }
            }

            using (var check = new SqliteCommand("SELECT COUNT(*) FROM Manufacturers", conn))
            {
                if ((long)check.ExecuteScalar() == 0)
                {
                    string insert = "INSERT INTO Manufacturers(Name) VALUES ('Puma'),('Adidas'),('Nike'),('Medved');";
                    using var insertCmd = new SqliteCommand(insert, conn);
                    insertCmd.ExecuteNonQuery();
                }
            }

            using (var check = new SqliteCommand("SELECT COUNT(*) FROM Suppliers", conn))
            {
                if ((long)check.ExecuteScalar() == 0)
                {
                    string insert = "INSERT INTO Suppliers(Name) VALUES ('АО ТИЗОЛ'),('АО Почта России'),('АО СДЭК'),('ИП Medved');";
                    using var insertCmd = new SqliteCommand(insert, conn);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private static SqliteConnection GetConn()
        {
            var conn = new SqliteConnection(cStr);
            conn.Open();

            using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys=ON;", conn);
            pragmaCmd.ExecuteNonQuery();
            return conn;
        }

        public static User GetUser(string login, string password)
        {
            using var conn = GetConn();

            using var cmd = new SqliteCommand("SELECT Id,Login,Password,FullName,Role FROM Users WHERE Login=@login AND Password=@password", conn);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@password", password);
            using var r = cmd.ExecuteReader();
            if (r.Read())
                return new User
                {
                    Id = r.GetInt32(0),
                    Login = r.GetString(1),
                    Password = r.GetString(2),
                    FullName = r.GetString(3),
                    Role = r.GetString(4)
                };
            return null;
        }

        public static List<User> GetUsers()
        {
            var list = new List<User>();
            using var conn = GetConn();
            using var cmd = new SqliteCommand("SELECT Id, FullName FROM Users ORDER BY FullName", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new User { Id = r.GetInt32(0), FullName = r.GetString(1) });
            return list;
        }

        public static List<Category> GetCategories()
        {
            var list = new List<Category>();
            using var conn = GetConn();
            using var cmd = new SqliteCommand("SELECT Id,Name FROM Categories ORDER BY Name", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Category
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1)
                });
            return list;
        }

        public static List<Manufacturer> GetManufacturers()
        {
            var list = new List<Manufacturer>();
            using var conn = GetConn();
            using var cmd = new SqliteCommand("SELECT Id,Name FROM Manufacturers ORDER BY Name", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Manufacturer
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1)
                });
            return list;
        }

        public static List<Supplier> GetSuppliers()
        {
            var list = new List<Supplier>();
            using var conn = GetConn();
            using var cmd = new SqliteCommand("SELECT Id,Name FROM Suppliers ORDER BY Name", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Supplier
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1)
                });
            return list;
        }

        public static List<Product> GetProducts(string search = "", int? supplierId = null, string sort = "ASC", int offset=0)
        {
            var list = new List<Product>();
            using var conn = GetConn();

            string sql = @"SELECT p.Id, p.Name, p.Description, p.CategoryId, c.Name AS CategoryName,
                p.ManufacturerId, m.Name AS ManufacturerName, p.SupplierId, s.Name AS SupplierName,
                p.Price, p.Unit, p.StockQuantity, p.Discount, p.ImagePath
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                LEFT JOIN Manufacturers m ON p.ManufacturerId = m.Id
                LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                WHERE 1=1
                ";

            var parameters = new List<SqliteParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND (p.Name LIKE @s OR p.Description LIKE @s OR c.Name LIKE @s OR m.Name LIKE @s OR s.Name LIKE @s)";
                parameters.Add(new SqliteParameter("@s", $"%{search}%"));
            }

            if (supplierId.HasValue)
            {
                sql += " AND p.SupplierId=@sId";
                parameters.Add(new SqliteParameter("@sId", supplierId.Value));
            }

            sql += $" ORDER BY p.StockQuantity {sort}";

            sql += $" LIMIT @limit OFFSET @offset";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@limit",5);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddRange(parameters.ToArray());
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Product
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1),
                    Description = r.IsDBNull(2) ? "" : r.GetString(2),
                    CategoryId = r.GetInt32(3),
                    CategoryName = r.GetString(4),
                    ManufacturerId = r.GetInt32(5),
                    ManufacturerName = r.GetString(6),
                    SupplierId = r.GetInt32(7),
                    SupplierName = r.GetString(8),
                    Price = r.GetDecimal(9),
                    Unit = r.GetString(10),
                    StockQuantity = r.GetInt32(11),
                    Discount = r.GetDecimal(12),
                    ImagePath = r.IsDBNull(13) ? null : r.GetString(13)
                });
            return list;
        }

        public static void InsertProduct(Product p)
        {
            using var conn = GetConn();
            string insert = @"INSERT INTO Products (Name, Description, CategoryId, ManufacturerId, SupplierId, Price, Unit, StockQuantity, Discount, ImagePath)
                VALUES (@n,@desc,@cId,@mId,@sId,@p,@u,@sq,@disc,@img);";
            using var cmd = new SqliteCommand(insert, conn);
            cmd.Parameters.AddWithValue("@n", p.Name);
            cmd.Parameters.AddWithValue("@desc", p.Description ?? "");
            cmd.Parameters.AddWithValue("@cId", p.CategoryId);
            cmd.Parameters.AddWithValue("@mId", p.ManufacturerId);
            cmd.Parameters.AddWithValue("@sId", p.SupplierId);
            cmd.Parameters.AddWithValue("@p", p.Price);
            cmd.Parameters.AddWithValue("@u", p.Unit);
            cmd.Parameters.AddWithValue("@sq", p.StockQuantity);
            cmd.Parameters.AddWithValue("@disc", p.Discount);
            cmd.Parameters.AddWithValue("@img", p.ImagePath ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateProduct(Product p)
        {
            using var conn = GetConn();
            string insert = @"UPDATE Products SET 
                Name = @n,
                Description = @desc,
                CategoryId = @cId,
                ManufacturerId = @mId,
                SupplierId = @sId,
                Price = @p,
                Unit = @u,
                StockQuantity = @sq,
                Discount = @disc,
                ImagePath = @img WHERE Id=@id;";
            using var cmd = new SqliteCommand(insert, conn);
            cmd.Parameters.AddWithValue("@id", p.Id);
            cmd.Parameters.AddWithValue("@n", p.Name);
            cmd.Parameters.AddWithValue("@desc", p.Description ?? "");
            cmd.Parameters.AddWithValue("@cId", p.CategoryId);
            cmd.Parameters.AddWithValue("@mId", p.ManufacturerId);
            cmd.Parameters.AddWithValue("@sId", p.SupplierId);
            cmd.Parameters.AddWithValue("@p", p.Price);
            cmd.Parameters.AddWithValue("@u", p.Unit);
            cmd.Parameters.AddWithValue("@sq", p.StockQuantity);
            cmd.Parameters.AddWithValue("@disc", p.Discount);
            cmd.Parameters.AddWithValue("@img", p.ImagePath ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteProduct(int id)
        {
            using var conn = GetConn();
            using var check = new SqliteCommand("SELECT COUNT(*) FROM OrderItems WHERE ProductId=@id;",conn);
            check.Parameters.AddWithValue("@id",id);
            if ((long)check.ExecuteScalar() > 0)
                throw new InvalidOperationException("Товар находится в списке заказов");
            using var cmd = new SqliteCommand("DELETE FROM Products WHERE Id=@id",conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }






        public static List<Order> GetOrders()
        {
            var list = new List<Order>();
            using var conn = GetConn();
            var sql = @"SELECT o.Id, o.OrderDate, o.Status, o.UserId, u.FullName 
                FROM Orders o
                LEFT JOIN Users u ON o.UserId = u.Id
                ORDER BY o.Id DESC";
            using var cmd = new SqliteCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Order
                {
                    Id = r.GetInt32(0),
                    OrderDate = r.IsDBNull(1) ? "" : r.GetString(1),
                    Status = r.GetString(2),
                    UserId = r.GetInt32(3),
                    FullName = r.IsDBNull(4) ? "Неизвестно" : r.GetString(4)
                });
            return list;
        }

        public static void InsertOrder(Order o)
        {
            using var conn = GetConn();
            var sql = "INSERT INTO Orders(OrderDate, Status, UserId) VALUES(@d, @s, @u)";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@d", o.OrderDate);
            cmd.Parameters.AddWithValue("@s", o.Status);
            cmd.Parameters.AddWithValue("@u", o.UserId);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateOrder(Order o)
        {
            using var conn = GetConn();
            var sql = "UPDATE Orders SET OrderDate=@d, Status=@s, UserId=@u WHERE Id=@id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", o.Id);
            cmd.Parameters.AddWithValue("@d", o.OrderDate);
            cmd.Parameters.AddWithValue("@s", o.Status);
            cmd.Parameters.AddWithValue("@u", o.UserId);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteOrder(int id)
        {
            using var conn = GetConn();
            // Сначала удаляем состав заказа (из-за внешних ключей)
            using var cmdItems = new SqliteCommand("DELETE FROM OrderItems WHERE OrderId=@id", conn);
            cmdItems.Parameters.AddWithValue("@id", id);
            cmdItems.ExecuteNonQuery();

            // Потом сам заказ
            using var cmdOrder = new SqliteCommand("DELETE FROM Orders WHERE Id=@id", conn);
            cmdOrder.Parameters.AddWithValue("@id", id);
            cmdOrder.ExecuteNonQuery();
        }
    }
}