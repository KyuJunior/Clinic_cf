using System;
using System.Globalization;
using System.Windows.Data;

namespace MedicalApp.Converters
{
    public class ZeroToEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int val && val == 0)
            {
                return string.Empty;
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string? str = value as string;
            if (string.IsNullOrWhiteSpace(str))
            {
                return 0;
            }
            if (int.TryParse(str, out int result))
            {
                return result;
            }
            return 0;
        }
    }
}
