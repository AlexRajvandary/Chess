using System;
using System.Globalization;
using System.Windows.Data;
using ChessWPF.ViewModels;

namespace ChessWPF.Converters
{
    public class PanelPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PanelPosition position && parameter is string param)
            {
                if (param == "Left")
                    return position == PanelPosition.Left;
                if (param == "Right")
                    return position == PanelPosition.Right;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string param)
            {
                if (param == "Left")
                    return PanelPosition.Left;
                if (param == "Right")
                    return PanelPosition.Right;
            }
            return Binding.DoNothing;
        }
    }
}

