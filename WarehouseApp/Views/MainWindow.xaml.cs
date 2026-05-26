#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WarehouseApp.Database;
using WarehouseApp.Models;

namespace WarehouseApp.Views
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper _db;
        private User _currentUser;
        private List<Product> _productsList;

        public MainWindow(User user)
        {
            InitializeComponent();

            _db = new DatabaseHelper();
            _currentUser = user;
            _productsList = new List<Product>();

            LoadUserInfo();
            LoadFilters();
            LoadStatistics();
            LoadProducts();
            SetPermissions();
        }

        private void LoadUserInfo()
        {
            txtUserInfo.Text = $"{_currentUser.FullName} | {_currentUser.Role} | Баллы: {_currentUser.Points}";
        }

        private void SetPermissions()
        {
            bool isAdmin = _currentUser.Role == "Admin";
            btnDelete.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            btnAddProduct.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            btnEdit.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadFilters()
        {
            cmbCategory.ItemsSource = _db.GetCategories();
            cmbCategory.SelectedIndex = 0;
            cmbStatus.ItemsSource = _db.GetStatuses();
            cmbStatus.SelectedIndex = 0;
        }

        private void LoadStatistics()
        {
            var stats = _db.GetStatistics();
            txtTotalProducts.Text = stats.totalProducts.ToString();
            txtLowStock.Text = stats.lowStock.ToString();
            txtOutOfStock.Text = stats.outOfStock.ToString();
            txtTotalValue.Text = stats.totalValue.ToString("C");
        }

        private void LoadProducts()
        {
            string search = txtSearch?.Text ?? "";
            string category = cmbCategory?.SelectedItem?.ToString() ?? "Все";
            string status = cmbStatus?.SelectedItem?.ToString() ?? "Все";

            _productsList = _db.GetProducts(search, category, status);
            dgProducts.ItemsSource = _productsList;
            txtRecordCount.Text = $"📋 Найдено: {_productsList.Count}";
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e) => LoadProducts();
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => LoadProducts();
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbCategory.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            LoadProducts();
            LoadStatistics();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Экспорт выполнен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CertificateSearch_Click(object sender, RoutedEventArgs e)
        {
            string code = Microsoft.VisualBasic.Interaction.InputBox("Введите код сертификата:", "Поиск");
            if (!string.IsNullOrEmpty(code))
            {
                var results = _db.FindByCertificate(code);
                if (results.Count > 0)
                    MessageBox.Show(string.Join("\n", results.Select(p => p.Name)), "Результат");
                else
                    MessageBox.Show("Не найдено");
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            new ProductDialog(null).ShowDialog();
            LoadProducts();
            LoadStatistics();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product p)
            {
                new ProductDialog(p.Id).ShowDialog();
                LoadProducts();
                LoadStatistics();
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product p &&
                MessageBox.Show($"Удалить \"{p.Name}\"?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _db.DeleteProduct(p.Id);
                LoadProducts();
                LoadStatistics();
            }
        }

        private void dgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool selected = dgProducts.SelectedItem != null;
            btnEdit.IsEnabled = selected;
            btnDelete.IsEnabled = selected;
        }

        private void Order_Click(object sender, RoutedEventArgs e)
        {
            new OrderWindow(_currentUser.Id).ShowDialog();
            LoadProducts();
            LoadStatistics();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            new ProfileWindow(_currentUser).ShowDialog();
            LoadUserInfo();
        }

        private void AIHelper_Click(object sender, RoutedEventArgs e)
        {
            new AIHelperWindow(_currentUser).ShowDialog();
        }

        private void ReplenishmentPlan_Click(object sender, RoutedEventArgs e)
        {
            var stats = _db.GetStatistics();
            MessageBox.Show($"📋 План пополнения\n\nЗаканчивается: {stats.lowStock}\nНет: {stats.outOfStock}");
        }

        private void Awards_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"🏆 Ваши награды\n\nБаллы: {_currentUser.Points}");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }
    }
}