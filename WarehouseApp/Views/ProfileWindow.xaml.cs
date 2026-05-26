#nullable disable

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WarehouseApp.Database;
using WarehouseApp.Models;

namespace WarehouseApp.Views
{
    public partial class ProfileWindow : Window
    {
        private readonly DatabaseHelper _db;
        private readonly User _currentUser;
        private int _selectedOrderId;

        public ProfileWindow(User user)
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
            // txtPoints.Text = _currentUser.Points.ToString();  // ← закомментировано или удалено
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
                txtAISubscription.Text = "❌ Нет активной подписки. Подключите нейросеть за 100 баллов.";
                btnSubscribe.Visibility = Visibility.Visible;
            }
        }

        private void LoadOrders()
        {
            try
            {
                var orders = _db.GetUserOrders(_currentUser.Id);
                dgOrders.ItemsSource = orders;
                txtOrdersCount.Text = orders.Count.ToString();
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
            if (dgOrders.SelectedItem is Order order)
            {
                _selectedOrderId = order.Id;
                btnShowReceipt.IsEnabled = true;

                var items = _db.GetOrderDetails(_selectedOrderId);
                string detailsText = "";

                if (items != null && items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        detailsText += $"📦 {item.ProductName}\n";
                        detailsText += $"   Количество: {item.Quantity} шт, Цена: {item.Price:C}, Сумма: {item.Total:C}\n\n";
                    }
                }
                else
                {
                    detailsText = $"Номер заказа: {order.OrderNumber}\n";
                    detailsText += $"Дата: {order.OrderDate:dd.MM.yyyy HH:mm}\n";
                    detailsText += $"Сумма: {order.TotalAmount:C}\n";
                    detailsText += $"Статус: {order.Status}\n";
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
                var order = _db.GetOrderInfo(_selectedOrderId);
                if (order != null)
                {
                    string orderDetails = $"Заказ от {order.OrderDate:dd.MM.yyyy HH:mm}\n";
                    orderDetails += $"Номер заказа: {order.OrderNumber}\n";
                    orderDetails += $"Получатель: {order.CustomerName}\n";
                    orderDetails += $"Телефон: {order.CustomerPhone}\n";
                    orderDetails += $"Адрес: {order.CustomerAddress}\n";
                    orderDetails += $"Оплата: {order.PaymentMethod}\n\n";
                    orderDetails += "Товары:\n";

                    var items = _db.GetOrderDetails(_selectedOrderId);
                    foreach (var item in items)
                    {
                        orderDetails += $"  • {item.ProductName} - {item.Quantity} шт x {item.Price:C} = {item.Total:C}\n";
                    }

                    var receiptWindow = new ReceiptWindow(orderDetails, order.TotalAmount);
                    receiptWindow.ShowDialog();
                }
            }
        }

        private void RefreshOrders_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            MessageBox.Show("Список заказов обновлен!", "Обновление",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Subscribe_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Points >= 100)
            {
                var result = MessageBox.Show(
                    "Подключить подписку на нейросеть?\nСтоимость: 100 баллов\n\n" +
                    "Возможности:\n• Прогнозирование спроса\n• Оптимизация закупок\n• Аналитика склада",
                    "Подписка на нейросеть",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _currentUser.Points -= 100;
                    _currentUser.IsSubscribedToAI = true;
                    _db.AddUserPoints(_currentUser.Id, -100);
                    _db.LogAction(_currentUser.Id, "Подписка", "Подключена подписка на нейросеть");

                    LoadProfile();
                    LoadAwards();

                    MessageBox.Show("✅ Подписка на нейросеть активирована!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show($"Недостаточно баллов! Нужно 100 баллов.\nУ вас: {_currentUser.Points} баллов",
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