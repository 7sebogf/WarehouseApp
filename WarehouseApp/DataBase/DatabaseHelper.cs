using System.Data;
using Microsoft.Data.SqlClient;
using WarehouseApp.Models;

namespace WarehouseApp.Database
{
    public class DatabaseHelper
    {
        private readonly string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=WarehouseDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public bool TestConnection()
        {
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ============ АВТОРИЗАЦИЯ И ПОЛЬЗОВАТЕЛИ ============

        public UserModel Authenticate(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                return null;

            using SqlConnection conn = GetConnection();
            conn.Open();

            string query = @"
                SELECT Id, Login, FullName, Role, Points, IsSubscribedToAI,
                       ISNULL(Email, '') AS Email, 
                       ISNULL(Phone, '') AS Phone, 
                       ISNULL(Gender, '') AS Gender, 
                       BirthDate, 
                       ISNULL(Address, '') AS Address, 
                       ISNULL(City, '') AS City,
                       ISNULL(RegistrationDate, GETDATE()) AS RegistrationDate
                FROM Users 
                WHERE Login = @login AND Password = @password";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@password", password);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                UserModel user = new UserModel();
                user.Id = reader.GetInt32(0);
                user.Login = reader.GetString(1);
                user.FullName = reader.GetString(2);
                user.Role = reader.GetString(3);
                user.Points = reader.GetInt32(4);
                user.IsSubscribedToAI = reader.GetBoolean(5);
                user.Email = reader.GetString(6);
                user.Phone = reader.GetString(7);
                user.Gender = reader.GetString(8);
                user.BirthDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9);
                user.Address = reader.GetString(10);
                user.City = reader.GetString(11);
                user.RegistrationDate = reader.GetDateTime(12);
                return user;
            }
            return null;
        }

        public bool Register(string login, string password, string fullName)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
                return false;

            using SqlConnection conn = GetConnection();
            conn.Open();

            string checkQuery = "SELECT COUNT(*) FROM Users WHERE Login = @login";
            using SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@login", login);
            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (exists > 0)
                return false;

            string insertQuery = @"
                INSERT INTO Users (Login, Password, FullName, Role, Points, IsSubscribedToAI, RegistrationDate) 
                VALUES (@login, @password, @fullName, 'User', 0, 0, GETDATE())";

            using SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@login", login);
            insertCmd.Parameters.AddWithValue("@password", password);
            insertCmd.Parameters.AddWithValue("@fullName", fullName);

            int result = insertCmd.ExecuteNonQuery();
            return result > 0;
        }

        public bool RegisterFull(UserModel user)
        {
            if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.FullName))
                return false;

            using SqlConnection conn = GetConnection();
            conn.Open();

            string checkQuery = "SELECT COUNT(*) FROM Users WHERE Login = @login";
            using SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@login", user.Login);
            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (exists > 0)
                return false;

            string insertQuery = @"
                INSERT INTO Users (Login, Password, FullName, Role, Points, IsSubscribedToAI, 
                                   Email, Phone, Gender, BirthDate, Address, City, RegistrationDate) 
                VALUES (@login, @password, @fullName, 'User', 0, 0,
                        @email, @phone, @gender, @birthDate, @address, @city, GETDATE())";

            using SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@login", user.Login);
            insertCmd.Parameters.AddWithValue("@password", user.Password);
            insertCmd.Parameters.AddWithValue("@fullName", user.FullName);
            insertCmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(user.Email) ? DBNull.Value : (object)user.Email);
            insertCmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(user.Phone) ? DBNull.Value : (object)user.Phone);
            insertCmd.Parameters.AddWithValue("@gender", string.IsNullOrEmpty(user.Gender) ? DBNull.Value : (object)user.Gender);
            insertCmd.Parameters.AddWithValue("@birthDate", user.BirthDate ?? (object)DBNull.Value);
            insertCmd.Parameters.AddWithValue("@address", string.IsNullOrEmpty(user.Address) ? DBNull.Value : (object)user.Address);
            insertCmd.Parameters.AddWithValue("@city", string.IsNullOrEmpty(user.City) ? DBNull.Value : (object)user.City);

            int result = insertCmd.ExecuteNonQuery();
            return result > 0;
        }

        public void LogAction(int userId, string actionType, string details)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();

            string query = @"
                INSERT INTO UserActions (UserId, ActionType, Details, Timestamp) 
                VALUES (@userId, @actionType, @details, GETDATE())";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@actionType", actionType ?? "Действие");
            cmd.Parameters.AddWithValue("@details", details ?? "");
            cmd.ExecuteNonQuery();
        }

        public void AddUserPoints(int userId, int points)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();

            string query = "UPDATE Users SET Points = Points + @points WHERE Id = @id";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@points", points);
            cmd.ExecuteNonQuery();
        }

        // ============ ТОВАРЫ ============

        public DataTable GetProducts(string searchText, string categoryFilter,
                                      string statusFilter, string sortBy,
                                      string sortOrder)
        {
            DataTable dt = new DataTable();

            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string query = @"
                    SELECT 
                        p.Id,
                        p.Name AS 'Название',
                        p.Article AS 'Артикул',
                        p.Weight AS 'ВесКг',
                        p.Quantity AS 'Количество',
                        p.Price AS 'Цена',
                        p.CertificateCode AS 'Сертификат',
                        ISNULL(s.Name, 'Unknown') AS 'Статус',
                        ISNULL(c.Name, N'Без категории') AS 'Категория'
                    FROM Products p
                    LEFT JOIN Statuses s ON p.StatusId = s.Id
                    LEFT JOIN Categories c ON p.CategoryId = c.Id
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += " AND (p.Name LIKE @search OR p.Article LIKE @search OR p.CertificateCode LIKE @search)";
                }

                if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "Все" && categoryFilter != "Без категории")
                {
                    query += " AND c.Name = @category";
                }

                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Все" && statusFilter != "Unknown")
                {
                    query += " AND s.Name = @status";
                }

                string validSortBy = sortBy switch
                {
                    "Название" => "p.Name",
                    "Артикул" => "p.Article",
                    "Количество" => "p.Quantity",
                    "Цена" => "p.Price",
                    "Вес" => "p.Weight",
                    "Статус" => "s.Name",
                    "Категория" => "c.Name",
                    _ => "p.Name"
                };

                query += " ORDER BY " + validSortBy + " " + (sortOrder == "ASC" ? "ASC" : "DESC");

                using SqlCommand cmd = new SqlCommand(query, conn);

                if (!string.IsNullOrEmpty(searchText))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");
                }
                if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "Все" && categoryFilter != "Без категории")
                {
                    cmd.Parameters.AddWithValue("@category", categoryFilter);
                }
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Все" && statusFilter != "Unknown")
                {
                    cmd.Parameters.AddWithValue("@status", statusFilter);
                }

                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetProducts Error: " + ex.Message);
            }

            return dt;
        }

        public bool AddProduct(string name, string article, decimal weight, int quantity,
                               decimal price, string categoryName, string statusName, string certificateCode)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();

            int? categoryId = GetIdByName("Categories", categoryName);
            int? statusId = GetIdByName("Statuses", statusName);

            string query = @"
                INSERT INTO Products (Name, Article, Weight, Quantity, Price, CategoryId, StatusId, CertificateCode)
                VALUES (@name, @article, @weight, @quantity, @price, @categoryId, @statusId, @certificate)";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@article", article);
            cmd.Parameters.AddWithValue("@weight", weight);
            cmd.Parameters.AddWithValue("@quantity", quantity);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@categoryId", categoryId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@statusId", statusId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@certificate", certificateCode ?? "");

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateProductFull(int id, string name, string article, decimal weight,
                                      int quantity, decimal price, string categoryName,
                                      string statusName, string certificateCode)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();

            int? categoryId = GetIdByName("Categories", categoryName);
            int? statusId = GetIdByName("Statuses", statusName);

            string query = @"
                UPDATE Products SET 
                    Name = @name,
                    Article = @article,
                    Weight = @weight,
                    Quantity = @quantity,
                    Price = @price,
                    CategoryId = @categoryId,
                    StatusId = @statusId,
                    CertificateCode = @certificate
                WHERE Id = @id";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@article", article);
            cmd.Parameters.AddWithValue("@weight", weight);
            cmd.Parameters.AddWithValue("@quantity", quantity);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@categoryId", categoryId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@statusId", statusId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@certificate", certificateCode ?? "");

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteProduct(int id)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();

            string query = "DELETE FROM Products WHERE Id = @id";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateProductQuantity(int productId, int deltaQuantity)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();

            string updateQuery = @"
                UPDATE Products 
                SET Quantity = Quantity + @delta 
                WHERE Id = @id AND Quantity + @delta >= 0";

            using SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
            updateCmd.Parameters.AddWithValue("@id", productId);
            updateCmd.Parameters.AddWithValue("@delta", deltaQuantity);

            int rowsAffected = updateCmd.ExecuteNonQuery();
            bool result = rowsAffected > 0;

            if (result)
            {
                string statusQuery = @"
                    UPDATE p
                    SET StatusId = 
                        CASE 
                            WHEN p.Quantity <= 0 THEN (SELECT Id FROM Statuses WHERE Name = 'Out of Stock')
                            WHEN p.Quantity <= p.MinStock THEN (SELECT Id FROM Statuses WHERE Name = 'Low Stock')
                            ELSE (SELECT Id FROM Statuses WHERE Name = 'In Stock')
                        END
                    FROM Products p
                    WHERE p.Id = @id";

                using SqlCommand statusCmd = new SqlCommand(statusQuery, conn);
                statusCmd.Parameters.AddWithValue("@id", productId);
                statusCmd.ExecuteNonQuery();
            }

            return result;
        }

        public void UpdateProductStatus(int productId)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();

            string query = @"
                UPDATE p
                SET StatusId = 
                    CASE 
                        WHEN p.Quantity <= 0 THEN (SELECT Id FROM Statuses WHERE Name = 'Out of Stock')
                        WHEN p.Quantity <= p.MinStock THEN (SELECT Id FROM Statuses WHERE Name = 'Low Stock')
                        ELSE (SELECT Id FROM Statuses WHERE Name = 'In Stock')
                    END
                FROM Products p
                WHERE p.Id = @id";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", productId);
            cmd.ExecuteNonQuery();
        }

        // ============ СПРАВОЧНИКИ ============

        public List<string> GetCategories()
        {
            List<string> categories = new List<string>();
            categories.Add("Все");
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string query = "SELECT Name FROM Categories ORDER BY Name";
                using SqlCommand cmd = new SqlCommand(query, conn);
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetCategories Error: " + ex.Message);
            }
            return categories;
        }

        public List<string> GetStatuses()
        {
            List<string> statuses = new List<string>();
            statuses.Add("Все");
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string query = "SELECT Name FROM Statuses ORDER BY Id";
                using SqlCommand cmd = new SqlCommand(query, conn);
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    statuses.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetStatuses Error: " + ex.Message);
            }
            return statuses;
        }

        private int? GetIdByName(string table, string name)
        {
            if (string.IsNullOrEmpty(name) || name == "Все" || name == "Без категории")
                return null;

            using SqlConnection conn = GetConnection();
            conn.Open();

            string query = "SELECT Id FROM " + table + " WHERE Name = @name";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", name);

            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : (int?)null;
        }

        // ============ СТАТИСТИКА ============

        public (int totalProducts, int lowStock, int outOfStock, decimal totalValue) GetStatistics()
        {
            int totalProducts = 0;
            int lowStock = 0;
            int outOfStock = 0;
            decimal totalValue = 0;

            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string totalQuery = "SELECT ISNULL(SUM(Quantity), 0) FROM Products";
                using SqlCommand totalCmd = new SqlCommand(totalQuery, conn);
                totalProducts = Convert.ToInt32(totalCmd.ExecuteScalar());

                string lowQuery = "SELECT COUNT(*) FROM Products WHERE Quantity > 0 AND Quantity <= MinStock";
                using SqlCommand lowCmd = new SqlCommand(lowQuery, conn);
                lowStock = Convert.ToInt32(lowCmd.ExecuteScalar());

                string outQuery = "SELECT COUNT(*) FROM Products WHERE Quantity = 0";
                using SqlCommand outCmd = new SqlCommand(outQuery, conn);
                outOfStock = Convert.ToInt32(outCmd.ExecuteScalar());

                string valueQuery = "SELECT ISNULL(SUM(Quantity * Price), 0) FROM Products";
                using SqlCommand valueCmd = new SqlCommand(valueQuery, conn);
                totalValue = Convert.ToDecimal(valueCmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetStatistics Error: " + ex.Message);
            }

            return (totalProducts, lowStock, outOfStock, totalValue);
        }

        // ============ ПОИСК ПО СЕРТИФИКАТУ ============

        public DataTable FindByCertificate(string certificateCode)
        {
            DataTable dt = new DataTable();
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string query = @"
                    SELECT 
                        p.Name AS 'Название',
                        p.Article AS 'Артикул',
                        p.Price AS 'Цена',
                        p.Quantity AS 'Количество',
                        ISNULL(w.Name, 'Не указан') AS 'Склад',
                        ISNULL(w.Region, 'Не указан') AS 'Регион'
                    FROM Products p
                    LEFT JOIN StockItems si ON p.Id = si.ProductId
                    LEFT JOIN Warehouses w ON si.WarehouseId = w.Id
                    WHERE p.CertificateCode = @certCode";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@certCode", certificateCode);

                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("FindByCertificate Error: " + ex.Message);
            }
            return dt;
        }

        // ============ ЗАКАЗЫ ============

        // Сохранение заказа в БД - исправленная версия
        public bool SaveOrder(int userId, string orderDetails, decimal total, string paymentMethod,
                              string customerName, string customerPhone, string customerAddress, List<CartItem> items)
        {
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string orderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                // Сохраняем заказ
                string orderQuery = @"
            INSERT INTO Orders (OrderNumber, UserId, TotalAmount, PaymentMethod, CustomerName, CustomerPhone, CustomerAddress, OrderDetails, Status, OrderDate)
            VALUES (@orderNumber, @userId, @total, @paymentMethod, @customerName, @customerPhone, @customerAddress, @orderDetails, 'Оплачен', GETDATE())";

                using SqlCommand orderCmd = new SqlCommand(orderQuery, conn);
                orderCmd.Parameters.AddWithValue("@orderNumber", orderNumber);
                orderCmd.Parameters.AddWithValue("@userId", userId);
                orderCmd.Parameters.AddWithValue("@total", total);
                orderCmd.Parameters.AddWithValue("@paymentMethod", paymentMethod);
                orderCmd.Parameters.AddWithValue("@customerName", customerName);
                orderCmd.Parameters.AddWithValue("@customerPhone", customerPhone);
                orderCmd.Parameters.AddWithValue("@customerAddress", customerAddress);
                orderCmd.Parameters.AddWithValue("@orderDetails", orderDetails);

                int result = orderCmd.ExecuteNonQuery();

                if (result > 0)
                {
                    // Получаем ID заказа
                    string getIdQuery = "SELECT Id FROM Orders WHERE OrderNumber = @orderNumber";
                    using SqlCommand getIdCmd = new SqlCommand(getIdQuery, conn);
                    getIdCmd.Parameters.AddWithValue("@orderNumber", orderNumber);
                    int orderId = Convert.ToInt32(getIdCmd.ExecuteScalar());

                    // Сохраняем позиции заказа
                    foreach (var item in items)
                    {
                        string itemQuery = @"
                    INSERT INTO OrderItems (OrderId, ProductName, Quantity, Price, Total)
                    VALUES (@orderId, @productName, @quantity, @price, @total)";

                        using SqlCommand itemCmd = new SqlCommand(itemQuery, conn);
                        itemCmd.Parameters.AddWithValue("@orderId", orderId);
                        itemCmd.Parameters.AddWithValue("@productName", item.Name);
                        itemCmd.Parameters.AddWithValue("@quantity", item.Quantity);
                        itemCmd.Parameters.AddWithValue("@price", item.Price);
                        itemCmd.Parameters.AddWithValue("@total", item.Total);
                        itemCmd.ExecuteNonQuery();
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveOrder Error: {ex.Message}");
                return false;
            }
        }

        // Получение всех заказов пользователя
        public DataTable GetUserOrders(int userId)
        {
            DataTable dt = new DataTable();
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string query = @"
            SELECT Id, OrderNumber, OrderDate, TotalAmount, PaymentMethod, Status
            FROM Orders 
            WHERE UserId = @userId 
            ORDER BY OrderDate DESC";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserOrders Error: {ex.Message}");
            }
            return dt;
        }

        // Получение деталей заказа
        public DataTable GetOrderDetails(int orderId)
        {
            DataTable dt = new DataTable();
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string query = @"
            SELECT ProductName, Quantity, Price, Total
            FROM OrderItems 
            WHERE OrderId = @orderId";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@orderId", orderId);

                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetOrderDetails Error: {ex.Message}");
            }
            return dt;
        }

        // Получение пользователя по ID
        public UserModel GetUserById(int userId)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();

            string query = @"
        SELECT Id, Login, FullName, Role, Points, IsSubscribedToAI,
               ISNULL(Email, '') AS Email, 
               ISNULL(Phone, '') AS Phone, 
               ISNULL(Gender, '') AS Gender, 
               BirthDate, 
               ISNULL(Address, '') AS Address, 
               ISNULL(City, '') AS City,
               ISNULL(RegistrationDate, GETDATE()) AS RegistrationDate
        FROM Users 
        WHERE Id = @id";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                UserModel user = new UserModel();
                user.Id = reader.GetInt32(0);
                user.Login = reader.GetString(1);
                user.FullName = reader.GetString(2);
                user.Role = reader.GetString(3);
                user.Points = reader.GetInt32(4);
                user.IsSubscribedToAI = reader.GetBoolean(5);
                user.Email = reader.GetString(6);
                user.Phone = reader.GetString(7);
                user.Gender = reader.GetString(8);
                user.BirthDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9);
                user.Address = reader.GetString(10);
                user.City = reader.GetString(11);
                user.RegistrationDate = reader.GetDateTime(12);
                return user;
            }
            return null;
        }

        // Получение рекомендаций по тегам
        public DataTable GetRecommendations(int productId)
        {
            DataTable dt = new DataTable();
            using SqlConnection conn = GetConnection();
            conn.Open();

            string query = @"
        SELECT TOP 5 DISTINCT 
            p.Id, p.Name AS 'Название', p.Price AS 'Цена', p.Quantity AS 'Количество'
        FROM Products p
        JOIN ProductTags pt ON p.Id = pt.ProductId
        WHERE pt.TagId IN (
            SELECT TagId FROM ProductTags WHERE ProductId = @productId
        )
        AND p.Id != @productId
        AND p.Quantity > 0
        ORDER BY NEWID()";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@productId", productId);
            dt.Load(cmd.ExecuteReader());
            return dt;
        }

        // Получение популярных рекомендаций
        public DataTable GetPopularRecommendations()
        {
            DataTable dt = new DataTable();
            using SqlConnection conn = GetConnection();
            conn.Open();

            string query = @"
        SELECT TOP 5 
            p.Id, p.Name AS 'Название', p.Price AS 'Цена', p.Quantity AS 'Количество'
        FROM Products p
        WHERE p.Quantity > 0
        ORDER BY (SELECT COUNT(*) FROM OrderItems oi WHERE oi.ProductName = p.Name) DESC";

            using SqlCommand cmd = new SqlCommand(query, conn);
            dt.Load(cmd.ExecuteReader());
            return dt;
        }

        // Получение полной информации о заказе
        public DataRow GetOrderInfo(int orderId)
        {
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string query = @"
            SELECT o.*, u.FullName, u.Email, u.Phone
            FROM Orders o
            JOIN Users u ON o.UserId = u.Id
            WHERE o.Id = @orderId";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@orderId", orderId);

                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                if (dt.Rows.Count > 0)
                    return dt.Rows[0];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetOrderInfo Error: {ex.Message}");
            }
            return null;
        }
    }
}