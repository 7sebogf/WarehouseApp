using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WarehouseApp.Database;
using WarehouseApp.Models;

namespace WarehouseApp.Views
{
    public partial class OrderWindow : Window
    {
        private readonly DatabaseHelper _db;
        private readonly int _userId;
        private DataTable _productsTable;
        private ObservableCollection<CartItem> _cart;

        public OrderWindow(int userId)
        {
            InitializeComponent();
            _db = new DatabaseHelper();
            _userId = userId;
            _cart = new ObservableCollection<CartItem>();
            lstCart.ItemsSource = _cart;

            LoadCategories();
            LoadProducts();
            LoadUserData();
        }

        private void LoadUserData()
        {
            var user = _db.GetUserById(_userId);
            if (user != null)
            {
                txtCustomerName.Text = user.FullName;
                txtCustomerPhone.Text = user.Phone;
                txtCustomerAddress.Text = user.Address;
            }
        }

        private void LoadCategories()
        {
            var categories = _db.GetCategories();
            cmbCategoryFilter.ItemsSource = categories;
            cmbCategoryFilter.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            string searchText = txtSearch.Text ?? "";
            string category = cmbCategoryFilter.SelectedItem?.ToString() ?? "Все";

            _productsTable = _db.GetProducts(searchText, category, "Все", "Название", "ASC");
            dgProducts.ItemsSource = _productsTable.DefaultView;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts();
        }

        private void cmbCategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
        }

        private void dgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAddToCart.IsEnabled = dgProducts.SelectedItem != null;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is DataRowView row)
            {
                int productId = Convert.ToInt32(row["Id"]);
                string name = row["Название"].ToString() ?? string.Empty;
                decimal price = Convert.ToDecimal(row["Цена"]);
                int available = Convert.ToInt32(row["Количество"]);

                if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 1)
                {
                    MessageBox.Show("❌ Введите корректное количество (минимум 1)!", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtQuantity.Focus();
                    txtQuantity.SelectAll();
                    return;
                }

                if (quantity > 999)
                {
                    MessageBox.Show("❌ Количество не может превышать 999!", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtQuantity.Focus();
                    txtQuantity.SelectAll();
                    return;
                }

                if (quantity > available)
                {
                    MessageBox.Show($"❌ Недостаточно товара! Доступно: {available} шт", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtQuantity.Focus();
                    return;
                }

                var existingItem = _cart.FirstOrDefault(x => x.ProductId == productId);
                if (existingItem != null)
                {
                    if (existingItem.Quantity + quantity > available)
                    {
                        MessageBox.Show($"❌ Недостаточно товара! Доступно: {available} шт", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    existingItem.Quantity += quantity;
                }
                else
                {
                    _cart.Add(new CartItem
                    {
                        ProductId = productId,
                        Name = name,
                        Price = price,
                        Quantity = quantity
                    });
                }

                txtQuantity.Text = "1";
                UpdateTotal();
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem item)
            {
                _cart.Remove(item);
                UpdateTotal();
            }
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count > 0)
            {
                var result = MessageBox.Show("Очистить корзину?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _cart.Clear();
                    UpdateTotal();
                }
            }
        }

        private void UpdateTotal()
        {
            decimal total = _cart.Sum(x => x.Total);
            txtTotal.Text = total.ToString("C");
        }

        private void SubmitOrder_Click(object sender, RoutedEventArgs e)
        {
            // ========== ВАЛИДАЦИЯ КОРЗИНЫ ==========
            if (_cart.Count == 0)
            {
                MessageBox.Show("❌ Корзина пуста! Добавьте товары.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ========== ВАЛИДАЦИЯ ФИО ==========
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("❌ Введите ФИО получателя!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomerName.Focus();
                return;
            }

            if (txtCustomerName.Text.Length < 5)
            {
                MessageBox.Show("❌ Введите полное ФИО (минимум 5 символов)!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomerName.Focus();
                return;
            }

            // ========== ВАЛИДАЦИЯ ТЕЛЕФОНА ==========
            if (string.IsNullOrWhiteSpace(txtCustomerPhone.Text))
            {
                MessageBox.Show("❌ Введите номер телефона!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomerPhone.Focus();
                return;
            }

            string cleanPhone = txtCustomerPhone.Text.Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
            if (!Regex.IsMatch(cleanPhone, @"^\d{10,11}$"))
            {
                MessageBox.Show("❌ Введите корректный номер телефона (10-11 цифр)!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomerPhone.Focus();
                txtCustomerPhone.SelectAll();
                return;
            }

            // ========== ВАЛИДАЦИЯ АДРЕСА ==========
            if (string.IsNullOrWhiteSpace(txtCustomerAddress.Text))
            {
                MessageBox.Show("❌ Введите адрес доставки!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomerAddress.Focus();
                return;
            }

            if (txtCustomerAddress.Text.Length < 10)
            {
                MessageBox.Show("❌ Введите полный адрес (минимум 10 символов)!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomerAddress.Focus();
                return;
            }

            string paymentMethod = "";
            if (rbCard.IsChecked == true) paymentMethod = "Банковская карта";
            else if (rbCash.IsChecked == true) paymentMethod = "Наличные";
            else if (rbOnline.IsChecked == true) paymentMethod = "Онлайн перевод";

            decimal total = _cart.Sum(x => x.Total);

            string orderDetails = $"Заказ от {DateTime.Now:dd.MM.yyyy HH:mm}\n";
            orderDetails += $"Получатель: {txtCustomerName.Text}\n";
            orderDetails += $"Телефон: {txtCustomerPhone.Text}\n";
            orderDetails += $"Адрес: {txtCustomerAddress.Text}\n";
            orderDetails += $"Оплата: {paymentMethod}\n\n";
            orderDetails += "Товары:\n";

            foreach (var item in _cart)
            {
                orderDetails += $"  • {item.Name} - {item.Quantity} шт x {item.Price:C} = {item.Total:C}\n";
            }

            orderDetails += $"\nИТОГО: {total:C}";

            _db.LogAction(_userId, "Оформление заказа", orderDetails);

            bool allSuccess = true;
            string errorMessage = "";

            foreach (var item in _cart)
            {
                bool success = _db.UpdateProductQuantity(item.ProductId, -item.Quantity);
                if (!success)
                {
                    allSuccess = false;
                    errorMessage = $"❌ Ошибка при списании товара: {item.Name}";
                    break;
                }
                _db.UpdateProductStatus(item.ProductId);
            }

            if (allSuccess)
            {
                _db.SaveOrder(_userId, orderDetails, total, paymentMethod,
                              txtCustomerName.Text, txtCustomerPhone.Text, txtCustomerAddress.Text, _cart.ToList());

                MessageBox.Show(
                    $"✅ ЗАКАЗ ОФОРМЛЕН!\n\n" +
                    $"Номер заказа: #{DateTime.Now:yyyyMMddHHmmss}\n" +
                    $"Сумма: {total:C}\n" +
                    $"Способ оплаты: {paymentMethod}",
                    "Заказ оформлен",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                var receiptWindow = new ReceiptWindow(orderDetails, total);
                receiptWindow.ShowDialog();

                int pointsToAdd = (int)(total / 1000);
                if (pointsToAdd > 0)
                {
                    _db.AddUserPoints(_userId, pointsToAdd);
                    _db.LogAction(_userId, "Награда", $"Получено {pointsToAdd} баллов за заказ на сумму {total:C}");
                }

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}