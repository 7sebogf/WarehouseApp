#nullable disable

using System;
using System.Linq;
using System.Windows;
using WarehouseApp.Database;
using WarehouseApp.Models;

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
            cmbCategory.ItemsSource = _db.GetCategories();
            cmbCategory.SelectedIndex = 0;
        }

        private void LoadStatuses()
        {
            cmbStatus.ItemsSource = _db.GetStatuses();
            cmbStatus.SelectedIndex = 0;
        }

        private void LoadProductData()
        {
            var product = _db.GetProducts().FirstOrDefault(p => p.Id == _productId);
            if (product != null)
            {
                txtName.Text = product.Name;
                txtArticle.Text = product.Article;
                txtWeight.Text = product.Weight.ToString();
                txtQuantity.Text = product.Quantity.ToString();
                txtPrice.Text = product.Price.ToString();
                txtCertificate.Text = product.CertificateCode;
                cmbCategory.SelectedItem = product.Category?.Name;
                cmbStatus.SelectedItem = product.Status?.Name;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название!");
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену!");
                return;
            }

            var product = new Product
            {
                Name = txtName.Text,
                Article = txtArticle.Text,
                Weight = decimal.TryParse(txtWeight.Text, out var w) ? w : 0,
                Quantity = int.TryParse(txtQuantity.Text, out var q) ? q : 0,
                Price = price,
                CertificateCode = txtCertificate.Text
            };

            if (_isEdit)
            {
                product.Id = _productId.Value;
                _db.UpdateProduct(product);
            }
            else
            {
                _db.AddProduct(product);
            }

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