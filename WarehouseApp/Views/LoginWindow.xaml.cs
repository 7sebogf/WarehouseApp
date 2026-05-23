using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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

            if (!_db.TestConnection())
            {
                MessageBox.Show("Не удалось подключиться к базе данных!\nПроверьте, запущен ли SQL Server LocalDB.",
                    "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void LoginTab_Click(object sender, RoutedEventArgs e)
        {
            loginPanel.Visibility = Visibility.Visible;
            registerPanel.Visibility = Visibility.Collapsed;

            btnLoginTab.Background = System.Windows.Media.Brushes.LightBlue;
            btnLoginTab.Foreground = System.Windows.Media.Brushes.White;
            btnRegisterTab.Background = System.Windows.Media.Brushes.LightGray;
            btnRegisterTab.Foreground = System.Windows.Media.Brushes.Gray;

            ClearErrors();
        }

        private void RegisterTab_Click(object sender, RoutedEventArgs e)
        {
            loginPanel.Visibility = Visibility.Collapsed;
            registerPanel.Visibility = Visibility.Visible;

            btnRegisterTab.Background = System.Windows.Media.Brushes.LightBlue;
            btnRegisterTab.Foreground = System.Windows.Media.Brushes.White;
            btnLoginTab.Background = System.Windows.Media.Brushes.LightGray;
            btnLoginTab.Foreground = System.Windows.Media.Brushes.Gray;

            ClearErrors();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login))
            {
                ShowError("Введите логин!", true);
                txtLogin.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Введите пароль!", true);
                txtPassword.Focus();
                return;
            }

            UserModel user = _db.Authenticate(login, password);

            if (user != null)
            {
                _db.LogAction(user.Id, "Вход", $"Успешная авторизация: {login}");

                MainWindow mainWindow = new MainWindow(user);
                mainWindow.Show();
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
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string gender = (cmbGender.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            if (gender == "Не выбран") gender = "";
            DateTime? birthDate = dpBirthDate.SelectedDate;
            string city = txtCity.Text.Trim();
            string address = txtAddress.Text.Trim();

            // ========== ВАЛИДАЦИЯ ЛОГИНА ==========
            if (string.IsNullOrEmpty(login))
            {
                ShowError("Введите логин!", false);
                txtRegLogin.Focus();
                return;
            }

            if (login.Length < 3)
            {
                ShowError("Логин должен содержать минимум 3 символа!", false);
                txtRegLogin.Focus();
                return;
            }

            if (login.Length > 50)
            {
                ShowError("Логин не должен превышать 50 символов!", false);
                txtRegLogin.Focus();
                return;
            }

            if (!Regex.IsMatch(login, @"^[a-zA-Z0-9_]+$"))
            {
                ShowError("Логин может содержать только буквы, цифры и знак подчеркивания!", false);
                txtRegLogin.Focus();
                return;
            }

            // ========== ВАЛИДАЦИЯ ПАРОЛЯ ==========
            if (string.IsNullOrEmpty(password))
            {
                ShowError("Введите пароль!", false);
                txtRegPassword.Focus();
                return;
            }

            if (password.Length < 4)
            {
                ShowError("Пароль должен содержать минимум 4 символа!", false);
                txtRegPassword.Focus();
                return;
            }

            if (password.Length > 100)
            {
                ShowError("Пароль не должен превышать 100 символов!", false);
                txtRegPassword.Focus();
                return;
            }

            if (password != confirm)
            {
                ShowError("Пароли не совпадают!", false);
                txtRegPassword.Focus();
                return;
            }

            // ========== ВАЛИДАЦИЯ ФИО ==========
            if (string.IsNullOrEmpty(fullName))
            {
                ShowError("Введите ФИО!", false);
                txtFullName.Focus();
                return;
            }

            if (fullName.Length < 5)
            {
                ShowError("Введите полное ФИО (минимум 5 символов)!", false);
                txtFullName.Focus();
                return;
            }

            if (fullName.Length > 200)
            {
                ShowError("ФИО не должно превышать 200 символов!", false);
                txtFullName.Focus();
                return;
            }

            // ========== ВАЛИДАЦИЯ EMAIL ==========
            if (!string.IsNullOrEmpty(email))
            {
                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    ShowError("Введите корректный email (пример: name@mail.ru)!", false);
                    txtEmail.Focus();
                    return;
                }
            }

            // ========== ВАЛИДАЦИЯ ТЕЛЕФОНА ==========
            if (!string.IsNullOrEmpty(phone))
            {
                string cleanPhone = phone.Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                if (!Regex.IsMatch(cleanPhone, @"^\d{10,11}$"))
                {
                    ShowError("Введите корректный номер телефона (10-11 цифр)!", false);
                    txtPhone.Focus();
                    return;
                }
            }

            // ========== ВАЛИДАЦИЯ ДАТЫ РОЖДЕНИЯ ==========
            if (birthDate.HasValue)
            {
                if (birthDate.Value > DateTime.Now)
                {
                    ShowError("Дата рождения не может быть в будущем!", false);
                    dpBirthDate.Focus();
                    return;
                }

                if (birthDate.Value < DateTime.Now.AddYears(-120))
                {
                    ShowError("Некорректная дата рождения!", false);
                    dpBirthDate.Focus();
                    return;
                }
            }

            // ========== ВАЛИДАЦИЯ ГОРОДА ==========
            if (!string.IsNullOrEmpty(city) && city.Length > 100)
            {
                ShowError("Название города не должно превышать 100 символов!", false);
                txtCity.Focus();
                return;
            }

            // ========== ВАЛИДАЦИЯ АДРЕСА ==========
            if (!string.IsNullOrEmpty(address) && address.Length > 500)
            {
                ShowError("Адрес не должен превышать 500 символов!", false);
                txtAddress.Focus();
                return;
            }

            var newUser = new UserModel
            {
                Login = login,
                Password = password,
                FullName = fullName,
                Email = email,
                Phone = phone,
                Gender = gender,
                BirthDate = birthDate,
                City = city,
                Address = address
            };

            bool success = _db.RegisterFull(newUser);

            if (success)
            {
                MessageBox.Show($"✅ Регистрация успешна!\n\nДобро пожаловать, {fullName}!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                LoginTab_Click(null, null);
                ClearRegisterForm();
            }
            else
            {
                ShowError("❌ Пользователь с таким логином уже существует!", false);
            }
        }

        private void ClearRegisterForm()
        {
            txtRegLogin.Clear();
            txtRegPassword.Clear();
            txtConfirmPassword.Clear();
            txtFullName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            cmbGender.SelectedIndex = 0;
            dpBirthDate.SelectedDate = null;
            txtCity.Clear();
            txtAddress.Clear();
        }

        private void ShowError(string message, bool isLogin)
        {
            if (isLogin)
            {
                txtLoginError.Text = message;
                txtLoginError.Visibility = Visibility.Visible;
            }
            else
            {
                txtRegisterError.Text = message;
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