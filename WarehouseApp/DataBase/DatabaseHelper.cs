#nullable disable

using Microsoft.EntityFrameworkCore;
using System.Windows;
using WarehouseApp.Data;
using WarehouseApp.Models;

namespace WarehouseApp.Database
{
    public class DatabaseHelper
    {
        private readonly AppDbContext _context;

        public DatabaseHelper()
        {
            try
            {
                _context = new AppDbContext();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания DbContext: {ex.Message}");
                throw;
            }
        }

        public bool TestConnection()
        {
            try
            {
                return _context.Database.CanConnect();
            }
            catch
            {
                return false;
            }
        }

        public User? Authenticate(string login, string password)
        {
            return _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
        }

        public User? GetUserById(int userId)
        {
            return _context.Users.Find(userId);
        }

        public bool Register(string login, string password, string fullName)
        {
            if (_context.Users.Any(u => u.Login == login))
                return false;

            _context.Users.Add(new User
            {
                Login = login,
                Password = password,
                FullName = fullName,
                Role = "User",
                Points = 0,
                IsSubscribedToAI = false,
                RegistrationDate = DateTime.Now
            });
            return _context.SaveChanges() > 0;
        }

        public bool RegisterFull(User user)
        {
            if (_context.Users.Any(u => u.Login == user.Login))
                return false;

            user.Role = "User";
            user.Points = 0;
            user.IsSubscribedToAI = false;
            user.RegistrationDate = DateTime.Now;

            _context.Users.Add(user);
            return _context.SaveChanges() > 0;
        }

        public void AddUserPoints(int userId, int points)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                user.Points += points;
                _context.SaveChanges();
            }
        }

        public void LogAction(int userId, string actionType, string details)
        {
            _context.UserActions.Add(new UserAction
            {
                UserId = userId,
                ActionType = actionType,
                Details = details,
                Timestamp = DateTime.Now
            });
            _context.SaveChanges();
        }

        public List<Product> GetProducts(string searchText = "", string categoryFilter = "",
                                 string statusFilter = "", string sortBy = "Название",
                                 string sortOrder = "ASC")
        {
            try
            {
                // ОБЯЗАТЕЛЬНО загружаем статус и категорию
                var query = _context.Products
                    .Include(p => p.Status)
                    .Include(p => p.Category)
                    .AsQueryable();

                // Поиск
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(p => p.Name.Contains(searchText) ||
                                             (p.Article != null && p.Article.Contains(searchText)) ||
                                             (p.CertificateCode != null && p.CertificateCode.Contains(searchText)));
                }

                // Фильтр по категории
                if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "Все")
                {
                    query = query.Where(p => p.Category != null && p.Category.Name == categoryFilter);
                }

                // Фильтр по статусу
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Все")
                {
                    query = query.Where(p => p.Status != null && p.Status.Name == statusFilter);
                }

                // Сортировка
                query = sortBy switch
                {
                    "Название" => sortOrder == "ASC" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                    "Артикул" => sortOrder == "ASC" ? query.OrderBy(p => p.Article) : query.OrderByDescending(p => p.Article),
                    "Количество" => sortOrder == "ASC" ? query.OrderBy(p => p.Quantity) : query.OrderByDescending(p => p.Quantity),
                    "Цена" => sortOrder == "ASC" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                    _ => query.OrderBy(p => p.Name)
                };

                var allProducts = query.ToList();

                // Отладка
                System.Diagnostics.Debug.WriteLine($"Найдено товаров: {allProducts.Count}");

                return allProducts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                return new List<Product>();
            }
        }

        public bool AddProduct(Product product)
        {
            _context.Products.Add(product);
            return _context.SaveChanges() > 0;
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                // Находим существующий товар в базе
                var existingProduct = _context.Products.Find(product.Id);
                if (existingProduct == null) return false;

                // Обновляем только нужные поля (без замены всего объекта)
                existingProduct.Name = product.Name;
                existingProduct.Article = product.Article;
                existingProduct.Weight = product.Weight;
                existingProduct.Quantity = product.Quantity;
                existingProduct.Price = product.Price;
                existingProduct.CertificateCode = product.CertificateCode;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.StatusId = product.StatusId;

                // Обновляем статус на основе количества
                if (existingProduct.Quantity <= 0)
                    existingProduct.StatusId = _context.Statuses.FirstOrDefault(s => s.Name == "Out of Stock")?.Id;
                else if (existingProduct.Quantity <= existingProduct.MinStock)
                    existingProduct.StatusId = _context.Statuses.FirstOrDefault(s => s.Name == "Low Stock")?.Id;
                else
                    existingProduct.StatusId = _context.Statuses.FirstOrDefault(s => s.Name == "In Stock")?.Id;

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateProduct Error: {ex.Message}");
                return false;
            }
        }

        public bool DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            return _context.SaveChanges() > 0;
        }

        public bool UpdateProductQuantity(int productId, int deltaQuantity)
        {
            try
            {
                var product = _context.Products.Find(productId);
                if (product == null) return false;

                // Обновляем количество
                product.Quantity += deltaQuantity;

                // Обновляем статус вручную (без триггера)
                if (product.Quantity <= 0)
                    product.StatusId = _context.Statuses.FirstOrDefault(s => s.Name == "Out of Stock")?.Id;
                else if (product.Quantity <= product.MinStock)
                    product.StatusId = _context.Statuses.FirstOrDefault(s => s.Name == "Low Stock")?.Id;
                else
                    product.StatusId = _context.Statuses.FirstOrDefault(s => s.Name == "In Stock")?.Id;

                // Используем ExecuteSqlRaw вместо SaveChanges для обхода триггера
                var sql = "UPDATE Products SET Quantity = @p0, StatusId = @p1 WHERE Id = @p2";
                var rowsAffected = _context.Database.ExecuteSqlRaw(sql, product.Quantity, product.StatusId, productId);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateProductQuantity Error: {ex.Message}");
                return false;
            }
        }

        public List<string> GetCategories()
        {
            var categories = _context.Categories.Select(c => c.Name).ToList();
            categories.Insert(0, "Все");
            return categories;
        }

        public List<string> GetStatuses()
        {
            var statuses = _context.Statuses.Select(s => s.Name).ToList();
            statuses.Insert(0, "Все");
            return statuses;
        }

        public (int totalProducts, int lowStock, int outOfStock, decimal totalValue) GetStatistics()
        {
            return (
                totalProducts: _context.Products.Sum(p => p.Quantity),
                lowStock: _context.Products.Count(p => p.Quantity > 0 && p.Quantity <= p.MinStock),
                outOfStock: _context.Products.Count(p => p.Quantity == 0),
                totalValue: _context.Products.Sum(p => p.Quantity * p.Price)
            );
        }

        public void SaveOrder(int userId, string orderDetails, decimal total, string paymentMethod,
                              string customerName, string customerPhone, string customerAddress, List<CartItem> items)
        {
            var order = new Order
            {
                OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                PaymentMethod = paymentMethod,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                CustomerAddress = customerAddress,
                OrderDetails = orderDetails,
                Status = "Оплачен",
                OrderItems = items.Select(i => new OrderItem
                {
                    ProductName = i.Name,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Total = i.Total
                }).ToList()
            };
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        public List<Order> GetUserOrders(int userId)
        {
            return _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public List<OrderItem> GetOrderDetails(int orderId)
        {
            return _context.OrderItems.Where(oi => oi.OrderId == orderId).ToList();
        }

        public Order? GetOrderInfo(int orderId)
        {
            return _context.Orders.Include(o => o.User).FirstOrDefault(o => o.Id == orderId);
        }

        public List<Product> FindByCertificate(string certificateCode)
        {
            return _context.Products
                .Where(p => p.CertificateCode != null && p.CertificateCode.Contains(certificateCode))
                .ToList();
        }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }
}