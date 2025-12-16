using System;
using System.Globalization;
using System.Windows.Data;
using ChessBoard.Models;

namespace ChessBoard.Converters
{
    public class ColorSchemeEqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return false;
            
            // values[0] is SelectedColorScheme from ViewModel
            // values[1] is the current ColorScheme from Tag
            if (values[0] is ColorScheme selected && values[1] is ColorScheme current)
            {
                return Equals(selected, current);
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // ConvertBack is not needed - we use Checked event to set SelectedColorScheme
            return new object[] { Binding.DoNothing, Binding.DoNothing };
        }
    }
}

