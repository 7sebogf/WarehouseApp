#nullable disable

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WarehouseApp.Database;
using WarehouseApp.Services;
using WarehouseApp.Models;

namespace WarehouseApp.Views
{
    public partial class AIHelperWindow : Window
    {
        private readonly DatabaseHelper _db;
        private readonly User _currentUser;  // ← User, не UserModel
        private readonly bool _hasAISubscription;

        public AIHelperWindow(User user)  // ← User, не UserModel
        {
            InitializeComponent();
            _db = new DatabaseHelper();
            _currentUser = user;
            _hasAISubscription = user.IsSubscribedToAI;

            if (!_hasAISubscription)
            {
                txtStatus.Text = "🔒 ДОСТУП ЗАКРЫТ";
                AddMessage("🔒 **Доступ к AI помощнику закрыт!**\n\n" +
                          "Для использования AI помощника необходимо:\n" +
                          "1. Перейти в Профиль\n" +
                          "2. Нажать кнопку 'Подключить нейросеть'\n" +
                          "3. Стоимость подписки: 100 баллов\n\n" +
                          $"⭐ Ваш баланс: {user.Points} баллов", false);

                btnAnalyzeStock.IsEnabled = false;
                btnRecommendReplenishment.IsEnabled = false;
                btnFindAlternatives.IsEnabled = false;
                btnForecastSales.IsEnabled = false;
                btnGenerateDescription.IsEnabled = false;
                txtInput.IsEnabled = false;
                btnSend.IsEnabled = false;
                return;
            }

            txtStatus.Text = "🧪 ТЕСТОВЫЙ РЕЖИМ";
            AddMessage("🤖 Привет! Я AI-помощник по управлению складом. Задай вопрос или выбери быструю команду!", false);
        }

        private void AddMessage(string text, bool isUser, bool isTyping = false)
        {
            Dispatcher.Invoke(() =>
            {
                var border = new Border
                {
                    Background = isUser ? new SolidColorBrush(Color.FromRgb(67, 97, 238)) : new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                    CornerRadius = new CornerRadius(15),
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = isUser ? new Thickness(50, 5, 10, 5) : new Thickness(10, 5, 50, 5),
                    HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                    Name = isTyping ? "TypingIndicator" : null
                };

                var textBlock = new TextBlock
                {
                    Text = text,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = isUser ? Brushes.White : new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                    MaxWidth = 350
                };

                border.Child = textBlock;
                ChatPanel.Children.Add(border);

                ChatScroll.ScrollToBottom();
            });
        }

        private void RemoveTypingIndicator()
        {
            Dispatcher.Invoke(() =>
            {
                for (int i = ChatPanel.Children.Count - 1; i >= 0; i--)
                {
                    if (ChatPanel.Children[i] is Border b && b.Name == "TypingIndicator")
                    {
                        ChatPanel.Children.RemoveAt(i);
                        break;
                    }
                }
            });
        }

        private async Task SendAndGetResponse(string userMessage)
        {
            if (!_hasAISubscription)
            {
                AddMessage("🔒 Доступ к AI помощнику закрыт. Оформите подписку в профиле!", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(userMessage)) return;

            AddMessage(userMessage, true);
            AddMessage("🤖 печатает...", false, true);

            try
            {
                var response = await GoogleGeminiAI.SendMessageAsync(userMessage);
                RemoveTypingIndicator();
                AddMessage(response, false);

                _db.LogAction(_currentUser.Id, "AI запрос", userMessage);
            }
            catch (Exception ex)
            {
                RemoveTypingIndicator();
                AddMessage($"❌ Ошибка: {ex.Message}", false);
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            var message = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            txtInput.Clear();
            await SendAndGetResponse(message);
        }

        private async void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                var message = txtInput.Text.Trim();
                if (!string.IsNullOrEmpty(message))
                {
                    txtInput.Clear();
                    await SendAndGetResponse(message);
                }
            }
        }

        private async void AnalyzeStock_Click(object sender, RoutedEventArgs e)
        {
            var stats = _db.GetStatistics();
            var message = $"анализ склада. Всего товаров: {stats.totalProducts}, заканчивается: {stats.lowStock}, нет в наличии: {stats.outOfStock}, общая стоимость: {stats.totalValue:C}";
            await SendAndGetResponse(message);
        }

        private async void RecommendReplenishment_Click(object sender, RoutedEventArgs e)
        {
            await SendAndGetResponse("пополнение склада. Что нужно заказать?");
        }

        private async void FindAlternatives_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Введите название товара:", "Поиск аналогов");
            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.Result))
            {
                await SendAndGetResponse($"аналоги товара {dialog.Result}");
            }
        }

        private async void ForecastSales_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Введите название товара:", "Прогноз продаж");
            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.Result))
            {
                await SendAndGetResponse($"прогноз продаж для {dialog.Result}");
            }
        }

        private async void GenerateDescription_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Введите название товара:", "Генерация описания");
            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.Result))
            {
                await SendAndGetResponse($"описание товара {dialog.Result}");
            }
        }
    }

    public class InputDialog : Window
    {
        public string Result { get; private set; } = "";

        public InputDialog(string prompt, string title)
        {
            Title = title;
            Width = 450;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.Children.Add(new TextBlock
            {
                Text = prompt,
                Margin = new Thickness(0, 0, 0, 15),
                TextWrapping = TextWrapping.Wrap
            });

            var txtInput = new TextBox
            {
                Height = 35,
                Margin = new Thickness(0, 0, 0, 15),
                Padding = new Thickness(8, 5, 8, 5)
            };
            grid.Children.Add(txtInput);
            Grid.SetRow(txtInput, 1);

            var stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;
            stack.HorizontalAlignment = HorizontalAlignment.Right;

            var btnOk = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 35,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(67, 97, 238)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            var btnCancel = new Button
            {
                Content = "Отмена",
                Width = 80,
                Height = 35,
                Background = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            btnOk.Click += (s, e) => { Result = txtInput.Text; DialogResult = true; Close(); };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };

            stack.Children.Add(btnOk);
            stack.Children.Add(btnCancel);
            grid.Children.Add(stack);
            Grid.SetRow(stack, 2);

            Content = grid;
        }
    }
}