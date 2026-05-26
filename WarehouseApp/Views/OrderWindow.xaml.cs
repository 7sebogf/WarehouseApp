#nullable disable

using System;
using System.Collections.ObjectModel;
using System.Linq;
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
            if (categories.Count > 0) cmbCategoryFilter.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            string searchText = txtSearch?.Text ?? "";
            string category = cmbCategoryFilter.SelectedItem?.ToString() ?? "Все";
            var products = _db.GetProducts(searchText, category, "", "Название", "ASC");
            dgProducts.ItemsSource = products;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadProducts();
        private void cmbCategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadProducts();

        private void dgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAddToCart.IsEnabled = dgProducts.SelectedItem != null;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product product)
            {
                if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 1)
                {
                    MessageBox.Show("Введите корректное количество!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (qty > product.Quantity)
                {
                    MessageBox.Show($"Недостаточно товара! Доступно: {product.Quantity} шт", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existing = _cart.FirstOrDefault(x => x.ProductId == product.Id);
                if (existing != null)
                    existing.Quantity += qty;
                else
                    _cart.Add(new CartItem
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = qty
                    });

                UpdateTotal();
                txtQuantity.Text = "1";
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
            if (_cart.Count > 0 && MessageBox.Show("Очистить корзину?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _cart.Clear();
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            txtTotal.Text = _cart.Sum(x => x.Total).ToString("C");
        }

        private void SubmitOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text) ||
                string.IsNullOrWhiteSpace(txtCustomerPhone.Text) ||
                string.IsNullOrWhiteSpace(txtCustomerAddress.Text))
            {
                MessageBox.Show("Заполните все данные получателя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string payment = rbCard.IsChecked == true ? "Банковская карта" :
                            rbCash.IsChecked == true ? "Наличные" : "Онлайн перевод";

            decimal total = _cart.Sum(x => x.Total);
            string details = string.Join("\n", _cart.Select(x => $"{x.Name} - {x.Quantity} шт x {x.Price:C} = {x.Total:C}"));

            foreach (var item in _cart)
            {
                _db.UpdateProductQuantity(item.ProductId, -item.Quantity);
            }

            _db.SaveOrder(_userId, details, total, payment,
                txtCustomerName.Text, txtCustomerPhone.Text, txtCustomerAddress.Text, _cart.ToList());

            MessageBox.Show($"✅ ЗАКАЗ ОФОРМЛЕН!\nСумма: {total:C}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            // Показываем чек
            var receipt = new ReceiptWindow(details, total);
            receipt.ShowDialog();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}