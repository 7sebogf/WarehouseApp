using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WarehouseApp.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string colorCode = value as string;
            if (string.IsNullOrEmpty(colorCode))
                return Brushes.Gray;

            try
            {
                return (SolidColorBrush)new BrushConverter().ConvertFromString(colorCode);
            }
            catch
            {
                return Brushes.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}