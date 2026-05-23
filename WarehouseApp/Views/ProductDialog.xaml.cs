using System;
using System.Data;
using System.Windows;
using WarehouseApp.Database;

namespace WarehouseApp.Views
{
    public partial class ProductDialog : Window
    {
        private readonly DatabaseHelper _db;
        private readonly int? _productId;
        private readonly bool _isEdit;

        public ProductDialog(int? productId = null)
        {
            InitializeComponent();
            _db = new DatabaseHelper();
            _productId = productId;
            _isEdit = productId.HasValue;

            LoadCategories();
            LoadStatuses();

            if (_isEdit)
            {
                txtTitle.Text = "✏️ Редактирование товара";
                LoadProductData();
            }
        }

        private void LoadCategories()
        {
            var categories = _db.GetCategories();
            cmbCategory.ItemsSource = categories;
            cmbCategory.SelectedIndex = 0;
        }

        private void LoadStatuses()
        {
            var statuses = _db.GetStatuses();
            cmbStatus.ItemsSource = statuses;
            cmbStatus.SelectedIndex = 0;
        }

        private void LoadProductData()
        {
            var dt = _db.GetProducts("", "", "", "Название", "ASC");
            var rows = dt.Select($"Id = {_productId}");

            if (rows.Length > 0)
            {
                var row = rows[0];
                txtName.Text = row["Название"].ToString();
                txtArticle.Text = row["Артикул"].ToString();
                txtWeight.Text = row["ВесКг"].ToString();
                txtQuantity.Text = row["Количество"].ToString();
                txtPrice.Text = row["Цена"].ToString();
                txtCertificate.Text = row["Сертификат"].ToString();

                cmbCategory.SelectedItem = row["Категория"].ToString();
                cmbStatus.SelectedItem = row["Статус"].ToString();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // ========== ВАЛИДАЦИЯ НАЗВАНИЯ ==========
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("❌ Введите название товара!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (txtName.Text.Length < 2)
            {
                MessageBox.Show("❌ Название товара должно содержать минимум 2 символа!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (txtName.Text.Length > 200)
            {
                MessageBox.Show("❌ Название товара не должно превышать 200 символов!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            // ========== ВАЛИДАЦИЯ АРТИКУЛА ==========
            if (string.IsNullOrWhiteSpace(txtArticle.Text))
            {
                MessageBox.Show("❌ Введите артикул товара!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtArticle.Focus();
                return;
            }

            if (txtArticle.Text.Length < 3)
            {
                MessageBox.Show("❌ Артикул должен содержать минимум 3 символа!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtArticle.Focus();
                return;
            }

            if (txtArticle.Text.Length > 50)
            {
                MessageBox.Show("❌ Артикул не должен превышать 50 символов!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtArticle.Focus();
                return;
            }

            // ========== ВАЛИДАЦИЯ ЦЕНЫ ==========
            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("❌ Введите корректную цену (число)!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                txtPrice.SelectAll();
                return;
            }

            if (price <= 0)
            {
                MessageBox.Show("❌ Цена должна быть больше 0!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                txtPrice.SelectAll();
                return;
            }

            if (price > 99999999)
            {
                MessageBox.Show("❌ Цена не может превышать 99 999 999 ₽!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return;
            }

            // ========== ВАЛИДАЦИЯ КОЛИЧЕСТВА ==========
            if (!int.TryParse(txtQuantity.Text, out int quantity))
            {
                MessageBox.Show("❌ Введите корректное количество (целое число)!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                txtQuantity.SelectAll();
                return;
            }

            if (quantity < 0)
            {
                MessageBox.Show("❌ Количество не может быть отрицательным!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                txtQuantity.SelectAll();
                return;
            }

            if (quantity > 99999)
            {
                MessageBox.Show("❌ Количество не может превышать 99 999!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                return;
            }

            // ========== ВАЛИДАЦИЯ ВЕСА ==========
            if (!decimal.TryParse(txtWeight.Text, out decimal weight))
            {
                MessageBox.Show("❌ Введите корректный вес (число)!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtWeight.Focus();
                txtWeight.SelectAll();
                return;
            }

            if (weight < 0)
            {
                MessageBox.Show("❌ Вес не может быть отрицательным!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtWeight.Focus();
                txtWeight.SelectAll();
                return;
            }

            if (weight > 10000)
            {
                MessageBox.Show("❌ Вес не может превышать 10 000 кг!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtWeight.Focus();
                return;
            }

            // ========== ВАЛИДАЦИЯ СЕРТИФИКАТА ==========
            string certificate = txtCertificate.Text.Trim();
            if (!string.IsNullOrEmpty(certificate) && certificate.Length > 100)
            {
                MessageBox.Show("❌ Код сертификата не должен превышать 100 символов!", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCertificate.Focus();
                return;
            }

            string category = cmbCategory.SelectedItem?.ToString() ?? "Без категории";
            string status = cmbStatus.SelectedItem?.ToString() ?? "Unknown";

            bool result;
            if (_isEdit)
            {
                result = _db.UpdateProductFull(_productId.Value, txtName.Text, txtArticle.Text,
                    weight, quantity, price, category, status, certificate);
            }
            else
            {
                result = _db.AddProduct(txtName.Text, txtArticle.Text, weight, quantity, price,
                    category, status, certificate);
            }

            if (result)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("❌ Ошибка при сохранении товара!\nВозможно, такой артикул уже существует.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}