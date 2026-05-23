using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QRCoder;

namespace WarehouseApp.Views
{
    public partial class ReceiptWindow : Window
    {
        private readonly string _orderDetails;
        private readonly decimal _total;
        private readonly string _orderNumber;
        private readonly DateTime _orderDate;
        private BitmapImage _qrBitmap;

        public ReceiptWindow(string orderDetails, decimal total)
        {
            InitializeComponent();

            _orderDetails = orderDetails;
            _total = total;
            _orderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            _orderDate = DateTime.Now;

            txtTotal.Text = total.ToString("C");
            LoadReceipt();
            GenerateQRCode();
        }

        private void GenerateQRCode()
        {
            try
            {
                // Формируем данные для QR-кода
                string qrData = $"Сумма: {_total:C}\n";
                qrData += $"Заказ: {_orderNumber}\n";
                qrData += $"Дата: {_orderDate:dd.MM.yyyy HH:mm}\n";
                qrData += $"Получатель: {GetCustomerName()}\n";
                qrData += $"Номер карты: **** **** **** 1234\n";
                qrData += $"Ссылка на оплату: https://pay.example.com/{_orderNumber}";

                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        using (System.Drawing.Bitmap qrBitmap = qrCode.GetGraphic(20))
                        {
                            _qrBitmap = ConvertBitmapToBitmapImage(qrBitmap);
                            imgQRCode.Source = _qrBitmap;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QR Error: {ex.Message}");
                // Создаем заглушку если не удалось создать QR
                CreatePlaceholderQR();
            }
        }

        private BitmapImage ConvertBitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void CreatePlaceholderQR()
        {
            // Создаем простой QR-код с текстом
            string qrText = $"Оплата заказа {_orderNumber}\nСумма: {_total:C}";

            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        using (System.Drawing.Bitmap qrBitmap = qrCode.GetGraphic(20))
                        {
                            _qrBitmap = ConvertBitmapToBitmapImage(qrBitmap);
                            imgQRCode.Source = _qrBitmap;
                        }
                    }
                }
            }
            catch
            {
                // Если совсем не получается - показываем текст вместо QR
                TextBlock textBlock = new TextBlock
                {
                    Text = $"QR-код\n{_orderNumber}\n{_total:C}",
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center,
                    Foreground = Brushes.Gray
                };
                // Не можем добавить в Image, так что просто показываем сообщение
                imgQRCode.Visibility = Visibility.Collapsed;
            }
        }

        private string GetCustomerName()
        {
            // Парсим имя из деталей заказа
            var lines = _orderDetails.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Получатель:"))
                {
                    return line.Replace("Получатель:", "").Trim();
                }
            }
            return "Клиент";
        }

        private void LoadReceipt()
        {
            spReceiptContent.Children.Clear();

            // Шапка
            AddHeader();
            AddSeparator();
            AddOrderInfo();
            AddSeparator();
            AddProducts();
            AddSeparator();
            AddFooter();
        }

        private void AddHeader()
        {
            var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            stack.Children.Add(new TextBlock
            {
                Text = "🏢 СКЛАДСКАЯ СИСТЕМА",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(67, 97, 238)),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            stack.Children.Add(new TextBlock
            {
                Text = "г. Москва, ул. Складская 1",
                FontSize = 11,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            stack.Children.Add(new TextBlock
            {
                Text = "Тел: +7(495)123-45-67",
                FontSize = 11,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            stack.Children.Add(new TextBlock
            {
                Text = "ИНН: 7701234567 / КПП: 770101001",
                FontSize = 11,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            spReceiptContent.Children.Add(stack);
        }

        private void AddSeparator()
        {
            spReceiptContent.Children.Add(new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0.5, 0, 0),
                Margin = new Thickness(0, 10, 0, 10)
            });
        }

        private void AddOrderInfo()
        {
            var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(new TextBlock
            {
                Text = "📄 ДАННЫЕ ЗАКАЗА",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            });

            var lines = _orderDetails.Split('\n');
            for (int i = 0; i < Math.Min(lines.Length, 15); i++)
            {
                if (!string.IsNullOrEmpty(lines[i]) && !lines[i].Contains("Товары:"))
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = lines[i],
                        FontSize = 12,
                        Margin = new Thickness(0, 2, 0, 2)
                    });
                }
            }
            spReceiptContent.Children.Add(stack);
        }

        private void AddProducts()
        {
            var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(new TextBlock
            {
                Text = "🛒 ТОВАРЫ",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            });

            // Заголовки
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = System.Windows.GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = System.Windows.GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = System.Windows.GridLength.Auto });

            headerGrid.Children.Add(new TextBlock { Text = "Наименование", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 5, 10, 5) });
            System.Windows.Controls.Grid.SetColumn(headerGrid.Children[0], 0);
            headerGrid.Children.Add(new TextBlock { Text = "Кол-во", FontWeight = FontWeights.Bold, Margin = new Thickness(10, 5, 10, 5) });
            System.Windows.Controls.Grid.SetColumn(headerGrid.Children[1], 1);
            headerGrid.Children.Add(new TextBlock { Text = "Цена", FontWeight = FontWeights.Bold, Margin = new Thickness(10, 5, 10, 5) });
            System.Windows.Controls.Grid.SetColumn(headerGrid.Children[2], 2);
            headerGrid.Children.Add(new TextBlock { Text = "Сумма", FontWeight = FontWeights.Bold, Margin = new Thickness(10, 5, 0, 5) });
            System.Windows.Controls.Grid.SetColumn(headerGrid.Children[3], 3);

            stack.Children.Add(headerGrid);

            var lines = _orderDetails.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("•") && line.Contains("шт"))
                {
                    var productGrid = new Grid();
                    productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
                    productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = System.Windows.GridLength.Auto });
                    productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = System.Windows.GridLength.Auto });
                    productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = System.Windows.GridLength.Auto });

                    var parts = line.Split(new[] { " - ", " шт x ", " = " }, StringSplitOptions.None);
                    string name = parts[0].Replace("  • ", "").Trim();
                    string quantity = parts.Length > 1 ? parts[1] : "";
                    string price = parts.Length > 2 ? parts[2].Replace("₽", "") : "";
                    string total = parts.Length > 3 ? parts[3].Replace("₽", "") : "";

                    productGrid.Children.Add(new TextBlock { Text = name, Margin = new Thickness(0, 3, 10, 3), TextWrapping = TextWrapping.Wrap });
                    System.Windows.Controls.Grid.SetColumn(productGrid.Children[0], 0);
                    productGrid.Children.Add(new TextBlock { Text = quantity, Margin = new Thickness(10, 3, 10, 3), TextAlignment = TextAlignment.Center });
                    System.Windows.Controls.Grid.SetColumn(productGrid.Children[1], 1);
                    productGrid.Children.Add(new TextBlock { Text = price + " ₽", Margin = new Thickness(10, 3, 10, 3), TextAlignment = TextAlignment.Right });
                    System.Windows.Controls.Grid.SetColumn(productGrid.Children[2], 2);
                    productGrid.Children.Add(new TextBlock { Text = total + " ₽", Margin = new Thickness(10, 3, 0, 3), TextAlignment = TextAlignment.Right, FontWeight = FontWeights.SemiBold });
                    System.Windows.Controls.Grid.SetColumn(productGrid.Children[3], 3);

                    stack.Children.Add(productGrid);
                }
            }
            spReceiptContent.Children.Add(stack);
        }

        private void AddFooter()
        {
            var stack = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            stack.Children.Add(new TextBlock
            {
                Text = "СПАСИБО ЗА ПОКУПКУ!",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });
            stack.Children.Add(new TextBlock
            {
                Text = "Товары надлежащего качества обмену и возврату не подлежат.",
                FontSize = 10,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(new TextBlock
            {
                Text = _orderDate.ToString("dd.MM.yyyy HH:mm:ss"),
                FontSize = 10,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            });
            spReceiptContent.Children.Add(stack);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FixedDocument document = new FixedDocument();
                    PageContent pageContent = new PageContent();
                    FixedPage fixedPage = new FixedPage();

                    var contentCopy = new StackPanel();
                    foreach (UIElement child in spReceiptContent.Children)
                    {
                        contentCopy.Children.Add(child);
                    }

                    fixedPage.Children.Add(contentCopy);
                    ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                    document.Pages.Add(pageContent);

                    printDialog.PrintDocument(document.DocumentPaginator, "Чек об оплате");
                    MessageBox.Show("Чек отправлен на печать!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка печати: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveQR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "PNG Image|*.png";
                saveDialog.FileName = $"QR_{_orderNumber}.png";

                if (saveDialog.ShowDialog() == true)
                {
                    if (_qrBitmap != null)
                    {
                        // Сохраняем QR-код
                        using (MemoryStream memory = new MemoryStream())
                        {
                            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(_qrBitmap));
                            encoder.Save(memory);

                            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(memory))
                            {
                                bitmap.Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }
                        MessageBox.Show($"QR-код сохранен!\n{saveDialog.FileName}", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения QR: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}