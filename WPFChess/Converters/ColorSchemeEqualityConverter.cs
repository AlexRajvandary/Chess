using System;
using System.Globalization;
using System.Windows.Data;
using ChessWPF.Models;

namespace ChessWPF.Converters
{
    public class ColorSchemeEqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return (values?.Length) == 2 && values[0] is ColorScheme selected
                && values[1] is ColorScheme current
                && Equals(selected, current);
        }

        /// <summary>
        /// Note: 
        /// In MultiBinding ConvertBack, we don't have access to the original values array.
        /// The actual update of SelectedColorScheme is handled by the Checked event handler
        /// in SettingsWindow.xaml.cs (ColorSchemeRadioButton_Checked)</summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>
        /// We return DoNothing to prevent WPF from trying to update the bindings,
        /// since the update is handled by the event handler instead
        /// </returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        { 
            return [Binding.DoNothing, Binding.DoNothing];
        }
    }
}