using System.Windows;
using System.Windows.Controls;
using ChessWPF.Models;
using ChessWPF.ViewModels;

namespace ChessWPF.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void ColorSchemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Tag is ColorScheme scheme)
            {
                if (DataContext is SettingsViewModel settingsViewModel)
                {
                    settingsViewModel.SelectedColorScheme = scheme;
                }
            }
        }
    }
}

