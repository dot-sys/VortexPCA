using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Vortex.UI.Converters
{
    public class RowForegroundConverter : IMultiValueConverter
    {
        private static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red);
        private static readonly SolidColorBrush DarkGoldenrodBrush = new SolidColorBrush(Color.FromRgb(184, 134, 11));

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return WhiteBrush;

            string status = values[0]?.ToString() ?? string.Empty;
            string signature = values[1]?.ToString() ?? string.Empty;

            if (!string.IsNullOrEmpty(status) &&
                status.IndexOf("Deleted", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return RedBrush;
            }

            if (!string.IsNullOrEmpty(status) &&
                status.IndexOf("Unknown", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return DarkGoldenrodBrush;
            }

            if (!string.IsNullOrEmpty(status) &&
                status.IndexOf("Present", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (string.IsNullOrEmpty(signature) ||
                    signature.IndexOf("Invalid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    signature.IndexOf("Unsigned", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    signature.IndexOf("Not Signed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    signature.IndexOf("Unknown", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return RedBrush;
                }
            }

            return WhiteBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("RowForegroundConverter does not support two-way binding");
        }
    }
}
