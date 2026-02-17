using System;
using System.Globalization;
using System.Windows.Data;

namespace Vortex.UI.Converters
{
    public abstract class StringContainsConverterBase : IValueConverter
    {
        protected abstract string SearchTerm { get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            string text = value.ToString();
            return !string.IsNullOrEmpty(text) && 
                   text.IndexOf(SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
