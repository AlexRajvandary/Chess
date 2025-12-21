using System;
using System.Globalization;
using System.Windows.Data;

namespace ChessWPF.Converters
{
    public class YearDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "All";
            if (value is int year)
                return year.ToString();
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
