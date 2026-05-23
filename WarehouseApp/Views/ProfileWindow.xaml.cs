using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using WarehouseApp.Database;
using WarehouseApp.Models;

namespace WarehouseApp.Views
{
    public partial class ProfileWindow : Window
    {
        private readonly DatabaseHelper _db;
        private readonly UserModel _currentUser;
        private DataTable _ordersTable;
        private int _selectedOrderId;

        public ProfileWindow(UserModel user)
        {
            InitializeComponent();
            _db = new DatabaseHelper();
            _currentUser = user;

            LoadProfile();
            LoadOrders();
            LoadAwards();
        }

        private void LoadProfile()
        {
            txtFullName.Text = _currentUser.FullName;
            txtLogin.Text = _currentUser.Login;
            txtRole.Text = _currentUser.Role == "Admin" ? "Администратор" : (_currentUser.Role == "Manager" ? "Менеджер" : "Пользователь");
            txtEmail.Text = string.IsNullOrEmpty(_currentUser.Email) ? "не указан" : _currentUser.Email;
            txtPhone.Text = string.IsNullOrEmpty(_currentUser.Phone) ? "не указан" : _currentUser.Phone;
            txtGender.Text = string.IsNullOrEmpty(_currentUser.Gender) ? "не указан" : _currentUser.Gender;
            txtBirthDate.Text = _currentUser.BirthDate?.ToString("dd.MM.yyyy") ?? "не указана";
            txtCity.Text = string.IsNullOrEmpty(_currentUser.City) ? "не указан" : _currentUser.City;
            txtAddress.Text = string.IsNullOrEmpty(_currentUser.Address) ? "не указан" : _currentUser.Address;
            txtRegDate.Text = _currentUser.RegistrationDate.ToString("dd.MM.yyyy");

            txtAvatar.Text = _currentUser.Gender == "Мужской" ? "👨" : (_currentUser.Gender == "Женский" ? "👩" : "👤");
            txtWelcomeName.Text = _currentUser.FullName;
            txtWelcomeRole.Text = _currentUser.Role == "Admin" ? "Администратор" : (_currentUser.Role == "Manager" ? "Менеджер" : "Пользователь");
            txtWelcomePoints.Text = _currentUser.Points.ToString();

            if (_currentUser.IsSubscribedToAI)
            {
                txtAISubscription.Text = "✅ Подписка активна! Доступны AI-аналитика и прогнозы.";
                btnSubscribe.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtAISubscription.Text = "❌ Нет активной подписки. Подключите нейросеть для аналитики склада.";
                btnSubscribe.Visibility = Visibility.Visible;
            }
        }

        private void LoadOrders()
        {
            try
            {
                _ordersTable = _db.GetUserOrders(_currentUser.Id);
                dgOrders.ItemsSource = _ordersTable.DefaultView;
                txtOrdersCount.Text = _ordersTable?.Rows.Count.ToString() ?? "0";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadOrders Error: {ex.Message}");
                txtOrdersCount.Text = "0";
            }
        }

        private void LoadAwards()
        {
            int points = _currentUser.Points;
            txtAwardsInfo.Text =
                $"⭐ ТЕКУЩИЕ БАЛЛЫ: {points}\n\n" +
                $"🏅 ДОСТИЖЕНИЯ:\n" +
                $"   🎖️ Новичок - 10 баллов {(10 <= points ? "✓ ПОЛУЧЕНО" : "✗ не получено")}\n" +
                $"   🏅 Активный пользователь - 50 баллов {(50 <= points ? "✓ ПОЛУЧЕНО" : "✗ не получено")}\n" +
                $"   ⭐ Мастер склада - 100 баллов {(100 <= points ? "✓ ПОЛУЧЕНО" : "✗ не получено")}\n" +
                $"   👑 Эксперт - 150 баллов {(150 <= points ? "✓ ПОЛУЧЕНО" : "✗ не получено")}";
        }

        private void DgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrders.SelectedItem is DataRowView row)
            {
                _selectedOrderId = Convert.ToInt32(row["Id"]);
                btnShowReceipt.IsEnabled = true;

                var details = _db.GetOrderDetails(_selectedOrderId);
                string detailsText = "";

                if (details != null && details.Rows.Count > 0)
                {
                    foreach (DataRow item in details.Rows)
                    {
                        detailsText += $"📦 {item["ProductName"]} - {item["Quantity"]} шт x {Convert.ToDecimal(item["Price"]):C} = {Convert.ToDecimal(item["Total"]):C}\n";
                    }
                }
                else
                {
                    detailsText = $"Номер заказа: {row["OrderNumber"]}\nСумма: {Convert.ToDecimal(row["TotalAmount"]):C}";
                }
                txtOrderDetails.Text = detailsText;
            }
            else
            {
                btnShowReceipt.IsEnabled = false;
                txtOrderDetails.Text = "Выберите заказ для просмотра";
            }
        }

        private void ShowReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrderId > 0)
            {
                var orderInfo = _db.GetOrderInfo(_selectedOrderId);
                if (orderInfo != null)
                {
                    string orderDetails = $"Заказ от {Convert.ToDateTime(orderInfo["OrderDate"]):dd.MM.yyyy HH:mm}\n";
                    orderDetails += $"Номер заказа: {orderInfo["OrderNumber"]}\n";
                    orderDetails += $"Получатель: {orderInfo["CustomerName"]}\n";
                    orderDetails += $"Телефон: {orderInfo["CustomerPhone"]}\n";
                    orderDetails += $"Адрес: {orderInfo["CustomerAddress"]}\n";
                    orderDetails += $"Оплата: {orderInfo["PaymentMethod"]}\n\nТовары:\n";

                    var items = _db.GetOrderDetails(_selectedOrderId);
                    foreach (DataRow item in items.Rows)
                    {
                        orderDetails += $"  • {item["ProductName"]} - {item["Quantity"]} шт x {Convert.ToDecimal(item["Price"]):C} = {Convert.ToDecimal(item["Total"]):C}\n";
                    }

                    decimal total = Convert.ToDecimal(orderInfo["TotalAmount"]);
                    var receiptWindow = new ReceiptWindow(orderDetails, total);
                    receiptWindow.ShowDialog();
                }
            }
        }

        private void Subscribe_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            // ========== ВАЛИДАЦИЯ БАЛЛОВ ==========
            if (_currentUser.Points >= 100)
            {
                var result = MessageBox.Show(
                    "🤖 **Подключить подписку на нейросеть?**\n\n" +
                    "💰 Стоимость: 100 баллов\n\n" +
                    "📊 Возможности:\n" +
                    "• Прогнозирование спроса\n" +
                    "• Оптимизация закупок\n" +
                    "• Аналитика склада\n" +
                    "• Рекомендации товаров\n\n" +
                    $"⭐ Ваш баланс: {_currentUser.Points} баллов\n\n" +
                    "После подключения AI помощник станет доступен!",
                    "Подписка на нейросеть",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _currentUser.Points -= 100;
                    _currentUser.IsSubscribedToAI = true;

                    using var conn = _db.GetConnection();
                    conn.Open();
                    string query = "UPDATE Users SET IsSubscribedToAI = 1, Points = @points WHERE Id = @id";
                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@points", _currentUser.Points);
                    cmd.Parameters.AddWithValue("@id", _currentUser.Id);
                    cmd.ExecuteNonQuery();

                    _db.LogAction(_currentUser.Id, "Подписка", "Подключена подписка на нейросеть");

                    LoadProfile();
                    LoadAwards();

                    MessageBox.Show("✅ **Подписка на нейросеть активирована!**\n\n" +
                                   "Теперь вам доступен AI помощник в главном меню.",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                int needed = 100 - _currentUser.Points;
                MessageBox.Show($"❌ **Недостаточно баллов!**\n\n" +
                               $"💰 Нужно: 100 баллов\n" +
                               $"⭐ У вас: {_currentUser.Points} баллов\n" +
                               $"📉 Не хватает: {needed} баллов\n\n" +
                               "💡 **Как получить баллы:**\n" +
                               "• Добавление товара +5 баллов\n" +
                               "• Редактирование товара +3 балла\n" +
                               "• Удаление товара +2 балла\n" +
                               "• Оформление заказа +10 баллов",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ProfileTab_Click(object sender, RoutedEventArgs e)
        {
            profilePanel.Visibility = Visibility.Visible;
            ordersPanel.Visibility = Visibility.Collapsed;
            awardsPanel.Visibility = Visibility.Collapsed;

            btnProfileTab.BorderBrush = System.Windows.Media.Brushes.LightBlue;
            btnProfileTab.Foreground = System.Windows.Media.Brushes.LightBlue;
            btnOrdersTab.BorderBrush = System.Windows.Media.Brushes.Transparent;
            btnOrdersTab.Foreground = System.Windows.Media.Brushes.Gray;
            btnAwardsTab.BorderBrush = System.Windows.Media.Brushes.Transparent;
            btnAwardsTab.Foreground = System.Windows.Media.Brushes.Gray;
        }

        private void OrdersTab_Click(object sender, RoutedEventArgs e)
        {
            profilePanel.Visibility = Visibility.Collapsed;
            ordersPanel.Visibility = Visibility.Visible;
            awardsPanel.Visibility = Visibility.Collapsed;
            LoadOrders();

            btnOrdersTab.BorderBrush = System.Windows.Media.Brushes.LightBlue;
            btnOrdersTab.Foreground = System.Windows.Media.Brushes.LightBlue;
            btnProfileTab.BorderBrush = System.Windows.Media.Brushes.Transparent;
            btnProfileTab.Foreground = System.Windows.Media.Brushes.Gray;
            btnAwardsTab.BorderBrush = System.Windows.Media.Brushes.Transparent;
            btnAwardsTab.Foreground = System.Windows.Media.Brushes.Gray;
        }

        private void AwardsTab_Click(object sender, RoutedEventArgs e)
        {
            profilePanel.Visibility = Visibility.Collapsed;
            ordersPanel.Visibility = Visibility.Collapsed;
            awardsPanel.Visibility = Visibility.Visible;

            btnAwardsTab.BorderBrush = System.Windows.Media.Brushes.LightBlue;
            btnAwardsTab.Foreground = System.Windows.Media.Brushes.LightBlue;
            btnProfileTab.BorderBrush = System.Windows.Media.Brushes.Transparent;
            btnProfileTab.Foreground = System.Windows.Media.Brushes.Gray;
            btnOrdersTab.BorderBrush = System.Windows.Media.Brushes.Transparent;
            btnOrdersTab.Foreground = System.Windows.Media.Brushes.Gray;
        }
    }
}