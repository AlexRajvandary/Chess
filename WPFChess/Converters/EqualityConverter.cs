using System;
using System.Globalization;
using System.Windows.Data;

namespace ChessWPF.Converters
{
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // When used in RadioButton, parameter will be the DataContext (ColorScheme) from Tag
            return Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // When RadioButton is checked, return the ColorScheme from parameter
            if (value is bool isChecked && isChecked && parameter != null)
            {
                return parameter;
            }
            return Binding.DoNothing;
        }
    }
}