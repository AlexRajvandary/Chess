using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChessWPF.Converters
{
    public class BooleanToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If parameter is provided, use it as the width when visible, otherwise default to 400
                double width = 400;
                if (parameter is string paramStr && double.TryParse(paramStr, out double paramWidth))
                {
                    width = paramWidth;
                }
                
                return boolValue ? new GridLength(width) : new GridLength(0);
            }
            return new GridLength(400);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gridLength)
            {
                return gridLength.Value > 0;
            }
            return true;
        }
    }
}
