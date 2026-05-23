using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using WarehouseApp.Database;
using WarehouseApp.Models;
using WarehouseApp.Services;

namespace WarehouseApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseHelper _db;
        private readonly UserModel _currentUser;
        private DataTable _productsTable;

        public MainWindow(UserModel user)
        {
            InitializeComponent();

            if (user == null)
            {
                GoToLogin();
                return;
            }

            _db = new DatabaseHelper();
            _currentUser = user;
            _productsTable = new DataTable();

            LoadUserInfo();
            LoadFilters();
            LoadStatistics();
            LoadProducts();

            SetPermissionsByRole();
            CheckNotifications();
        }

        private void GoToLogin()
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void LoadUserInfo()
        {
            if (txtUserInfo != null && _currentUser != null)
            {
                string genderIcon = "👤";
                if (_currentUser.Gender == "Мужской") genderIcon = "👨";
                else if (_currentUser.Gender == "Женский") genderIcon = "👩";

                txtUserInfo.Text = $"{genderIcon} {_currentUser.FullName} | Роль: {_currentUser.Role} | Баллы: {_currentUser.Points}";
            }
        }

        private void SetPermissionsByRole()
        {
            bool isAdmin = _currentUser.Role == "Admin";
            bool isManager = _currentUser.Role == "Manager";

            // Только админ может удалять
            if (btnDelete != null) btnDelete.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            // Админ и менеджер могут добавлять и редактировать
            if (btnAddProduct != null) btnAddProduct.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;
            if (btnEdit != null) btnEdit.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadFilters()
        {
            if (_db == null) return;

            if (cmbCategory != null)
            {
                cmbCategory.ItemsSource = _db.GetCategories();
                cmbCategory.SelectedIndex = 0;
            }

            if (cmbStatus != null)
            {
                cmbStatus.ItemsSource = _db.GetStatuses();
                cmbStatus.SelectedIndex = 0;
            }
        }

        private void LoadStatistics()
        {
            if (_db == null) return;

            var stats = _db.GetStatistics();

            if (txtTotalProducts != null) txtTotalProducts.Text = stats.totalProducts.ToString();
            if (txtLowStock != null) txtLowStock.Text = stats.lowStock.ToString();
            if (txtOutOfStock != null) txtOutOfStock.Text = stats.outOfStock.ToString();
            if (txtTotalValue != null) txtTotalValue.Text = stats.totalValue.ToString("C");
        }

        private void LoadProducts()
        {
            if (_db == null) return;

            string searchText = txtSearch?.Text ?? "";
            string category = cmbCategory?.SelectedItem?.ToString() ?? "Все";
            string status = cmbStatus?.SelectedItem?.ToString() ?? "Все";
            string sortBy = (cmbSortBy?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Название";
            string sortOrder = (cmbSortOrder?.SelectedItem as ComboBoxItem)?.Content?.ToString() == "▲ Возрастание" ? "ASC" : "DESC";

            _productsTable = _db.GetProducts(searchText, category, status, sortBy, sortOrder);

            if (dgProducts != null)
            {
                dgProducts.ItemsSource = _productsTable?.DefaultView;
            }

            if (txtRecordCount != null)
            {
                int count = _productsTable?.Rows.Count ?? 0;
                txtRecordCount.Text = $"📋 Найдено записей: {count}";
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e) => LoadProducts();
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => LoadProducts();
        private void Sort_Changed(object sender, SelectionChangedEventArgs e) => LoadProducts();

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (txtSearch != null) txtSearch.Text = "";
            if (cmbCategory != null) cmbCategory.SelectedIndex = 0;
            if (cmbStatus != null) cmbStatus.SelectedIndex = 0;
            if (cmbSortBy != null) cmbSortBy.SelectedIndex = 0;
            if (cmbSortOrder != null) cmbSortOrder.SelectedIndex = 0;

            LoadProducts();
            LoadStatistics();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (_db == null || _currentUser == null) return;

            try
            {
                int count = _productsTable?.Rows.Count ?? 0;
                string filename = $"Export_Products_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);

                StringBuilder sb = new StringBuilder();
                if (_productsTable != null && _productsTable.Rows.Count > 0)
                {
                    for (int i = 0; i < _productsTable.Columns.Count; i++)
                    {
                        sb.Append(_productsTable.Columns[i].ColumnName);
                        if (i < _productsTable.Columns.Count - 1) sb.Append(";");
                    }
                    sb.AppendLine();

                    foreach (DataRow row in _productsTable.Rows)
                    {
                        for (int i = 0; i < _productsTable.Columns.Count; i++)
                        {
                            sb.Append(row[i].ToString());
                            if (i < _productsTable.Columns.Count - 1) sb.Append(";");
                        }
                        sb.AppendLine();
                    }
                }

                File.WriteAllText(filepath, sb.ToString(), Encoding.UTF8);

                _db.LogAction(_currentUser.Id, "Экспорт", $"Выгружен отчет по товарам. {count} записей");
                MessageBox.Show($"Экспорт данных выполнен!\nФайл: {filename}\nСохранен на рабочем столе",
                    "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);

                AddPoints(5, "Экспорт данных");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CertificateSearch_Click(object sender, RoutedEventArgs e)
        {
            if (_db == null || _currentUser == null) return;

            string certCode = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите код сертификата:",
                "Поиск по сертификату",
                "CERT-");

            if (!string.IsNullOrEmpty(certCode))
            {
                DataTable result = _db.FindByCertificate(certCode);
                if (result != null && result.Rows.Count > 0)
                {
                    string message = $"📋 Результат поиска по сертификату {certCode}:\n\n";
                    foreach (DataRow row in result.Rows)
                    {
                        message += $"📦 {row["Название"]}\n";
                        message += $"   Артикул: {row["Артикул"]}, Цена: {row["Цена"]:C}\n";
                        message += $"   Количество: {row["Количество"]} шт\n";
                        message += $"   Склад: {row["Склад"]} ({row["Регион"]})\n\n";
                    }
                    MessageBox.Show(message, "Результат поиска", MessageBoxButton.OK, MessageBoxImage.Information);
                    _db.LogAction(_currentUser.Id, "Поиск", $"Поиск по сертификату: {certCode}");
                    AddPoints(2, "Поиск по сертификату");
                }
                else
                {
                    MessageBox.Show("Товар с таким сертификатом не найден!", "Результат поиска",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_db == null || _currentUser == null) return;

            var dialog = new ProductDialog(null);
            if (dialog.ShowDialog() == true)
            {
                LoadProducts();
                LoadStatistics();
                _db.LogAction(_currentUser.Id, "Добавление", "Добавлен новый товар");
                AddPoints(5, "Добавление нового товара");
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (_db == null || _currentUser == null) return;

            if (dgProducts?.SelectedItem is DataRowView row)
            {
                int id = Convert.ToInt32(row["Id"]);
                var dialog = new ProductDialog(id);
                if (dialog.ShowDialog() == true)
                {
                    LoadProducts();
                    LoadStatistics();
                    _db.LogAction(_currentUser.Id, "Редактирование", $"Отредактирован товар: {row["Название"]}");
                    AddPoints(3, "Редактирование товара");
                }
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_db == null || _currentUser == null) return;

            if (dgProducts?.SelectedItem is DataRowView row)
            {
                var result = MessageBox.Show($"Удалить товар \"{row["Название"]}\"?\nЭто действие необратимо!",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    int id = Convert.ToInt32(row["Id"]);
                    string name = row["Название"]?.ToString() ?? "";

                    if (_db.DeleteProduct(id))
                    {
                        LoadProducts();
                        LoadStatistics();
                        _db.LogAction(_currentUser.Id, "Удаление", $"Удален товар: {name}");
                        AddPoints(2, "Удаление товара");

                        MessageBox.Show("Товар успешно удален!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при удалении!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isSelected = dgProducts?.SelectedItem != null;
            if (btnEdit != null) btnEdit.IsEnabled = isSelected;
            if (btnDelete != null) btnDelete.IsEnabled = isSelected;
        }

        private void AddPoints(int points, string reason)
        {
            if (_currentUser == null) return;

            _currentUser.Points += points;

            using var conn = _db.GetConnection();
            conn.Open();
            string query = "UPDATE Users SET Points = @points WHERE Id = @id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@points", _currentUser.Points);
            cmd.Parameters.AddWithValue("@id", _currentUser.Id);
            cmd.ExecuteNonQuery();

            _db.LogAction(_currentUser.Id, "Награда", $"Получено {points} баллов: {reason}");

            LoadUserInfo();
        }

        private void CheckNotifications()
        {
            var stats = _db.GetStatistics();
            if (stats.lowStock > 0 || stats.outOfStock > 0)
            {
                string message = $"📢 **Уведомления склада:**\n\n";
                if (stats.lowStock > 0) message += $"⚠️ {stats.lowStock} товаров заканчиваются!\n";
                if (stats.outOfStock > 0) message += $"❌ {stats.outOfStock} товаров отсутствуют!\n";
                message += $"\n💡 Рекомендуется пополнить запасы.";

                MessageBox.Show(message, "Уведомления", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ReplenishmentPlan_Click(object sender, RoutedEventArgs e)
        {
            if (_db == null) return;

            var stats = _db.GetStatistics();
            MessageBox.Show(
                $"📋 **План пополнения склада**\n\n" +
                $"📦 Товаров с низким остатком: {stats.lowStock}\n" +
                $"❌ Отсутствующих товаров: {stats.outOfStock}\n\n" +
                $"**Рекомендации:**\n" +
                $"• Пополнить запасы товаров с остатком менее 5 шт\n" +
                $"• Заказать отсутствующие позиции\n" +
                $"• Проверить поставщиков по категориям",
                "Формирование плана пополнения",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            if (_currentUser != null)
            {
                _db.LogAction(_currentUser.Id, "План пополнения", "Сформирован план пополнения склада");
                AddPoints(5, "Формирование плана пополнения");
            }
        }

        private void Awards_Click(object sender, RoutedEventArgs e)
        {
            int points = _currentUser?.Points ?? 0;
            MessageBox.Show(
                $"🏆 **ВАШИ НАГРАДЫ**\n\n" +
                $"⭐ Текущие баллы: {points}\n\n" +
                $"**Достижения:**\n" +
                $"   🎖️ Новичок - 10 баллов {(10 <= points ? "✓ ПОЛУЧЕНО" : "✗ не получено")}\n" +
                $"   🏅 Активный пользователь - 50 баллов {(50 <= points ? "✓ ПОЛУЧЕНО" : "✗ не получено")}\n" +
                $"   ⭐ Мастер склада - 100 баллов {(100 <= points ? "✓ ПОЛУЧЕНО" : "✗ не получено")}\n" +
                $"   👑 Эксперт - 150 баллов {(150 <= points ? "✓ ПОЛУЧЕНО" : "✗ не получено")}\n\n" +
                $"**Как получить баллы:**\n" +
                $"   • Добавление товара +5 баллов\n" +
                $"   • Редактирование товара +3 балла\n" +
                $"   • Удаление товара +2 балла\n" +
                $"   • Поиск по сертификату +2 балла\n" +
                $"   • Экспорт данных +5 баллов\n" +
                $"   • Формирование плана +5 баллов\n" +
                $"   • Оформление заказа +10 баллов",
                "Награды",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                var profileWindow = new ProfileWindow(_currentUser);
                profileWindow.ShowDialog();
                LoadUserInfo();
            }
        }

        private void Order_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            var orderWindow = new OrderWindow(_currentUser.Id);
            if (orderWindow.ShowDialog() == true)
            {
                LoadProducts();
                LoadStatistics();
                _db.LogAction(_currentUser.Id, "Заказ", "Оформлен новый заказ");
                AddPoints(10, "Оформление заказа");
            }
        }

        private void AIHelper_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            var aiWindow = new AIHelperWindow(_currentUser);
            aiWindow.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (_db != null && _currentUser != null)
            {
                _db.LogAction(_currentUser.Id, "Выход", "Завершение сессии");
            }

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}