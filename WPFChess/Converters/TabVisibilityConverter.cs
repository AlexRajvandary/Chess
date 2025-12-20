using System;
using System.Globalization;
using System.Windows.Data;
using ChessWPF.ViewModels;

namespace ChessWPF.Converters
{
    public class TabVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SidePanelTab selectedTab && parameter is string tabName)
            {
                SidePanelTab targetTab = tabName switch
                {
                    "Game" => SidePanelTab.Game,
                    "Settings" => SidePanelTab.Settings,
                    "About" => SidePanelTab.About,
                    _ => SidePanelTab.Game
                };
                return selectedTab == targetTab ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

