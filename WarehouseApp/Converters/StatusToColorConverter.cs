using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WarehouseApp.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string ?? "";

            if (status.Contains("В наличии"))
                return new SolidColorBrush(Color.FromRgb(40, 167, 69)); // #28A745
            if (status.Contains("Заканчивается"))
                return new SolidColorBrush(Color.FromRgb(255, 193, 7)); // #FFC107
            if (status.Contains("Нет в наличии"))
                return new SolidColorBrush(Color.FromRgb(220, 53, 69)); // #DC3545
            if (status.Contains("Ожидается"))
                return new SolidColorBrush(Color.FromRgb(23, 162, 184)); // #17A2B8

            return new SolidColorBrush(Color.FromRgb(108, 117, 125)); // #6C757D
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}