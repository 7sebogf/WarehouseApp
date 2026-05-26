#nullable disable

using System;
using System.Text.RegularExpressions;
using System.Windows;
using WarehouseApp.Database;
using WarehouseApp.Models;

namespace WarehouseApp.Views
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseHelper _db;

        public LoginWindow()
        {
            InitializeComponent();
            _db = new DatabaseHelper();
        }

        private void LoginTab_Click(object sender, RoutedEventArgs e)
        {
            loginPanel.Visibility = Visibility.Visible;
            registerPanel.Visibility = Visibility.Collapsed;
            ClearErrors();
        }

        private void RegisterTab_Click(object sender, RoutedEventArgs e)
        {
            loginPanel.Visibility = Visibility.Collapsed;
            registerPanel.Visibility = Visibility.Visible;
            ClearErrors();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль!", true);
                return;
            }

            var user = _db.Authenticate(login, password);
            if (user != null)
            {
                new MainWindow(user).Show();
                Close();
            }
            else
            {
                ShowError("Неверный логин или пароль!", true);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string login = txtRegLogin.Text.Trim();
            string password = txtRegPassword.Password;
            string confirm = txtConfirmPassword.Password;
            string fullName = txtFullName.Text.Trim();

            if (string.IsNullOrEmpty(login) || login.Length < 3)
            {
                ShowError("Логин не менее 3 символов!", false);
                return;
            }
            if (string.IsNullOrEmpty(password) || password.Length < 4)
            {
                ShowError("Пароль не менее 4 символов!", false);
                return;
            }
            if (password != confirm)
            {
                ShowError("Пароли не совпадают!", false);
                return;
            }
            if (string.IsNullOrEmpty(fullName))
            {
                ShowError("Введите ФИО!", false);
                return;
            }

            if (_db.Register(login, password, fullName))
            {
                MessageBox.Show("Регистрация успешна!");
                LoginTab_Click(null, null);
            }
            else
            {
                ShowError("Пользователь уже существует!", false);
            }
        }

        private void ShowError(string msg, bool isLogin)
        {
            if (isLogin)
            {
                txtLoginError.Text = msg;
                txtLoginError.Visibility = Visibility.Visible;
            }
            else
            {
                txtRegisterError.Text = msg;
                txtRegisterError.Visibility = Visibility.Visible;
            }
        }

        private void ClearErrors()
        {
            txtLoginError.Visibility = Visibility.Collapsed;
            txtRegisterError.Visibility = Visibility.Collapsed;
        }
    }
}