using System.Windows;
using System.Windows.Controls;

namespace ChessWPF.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void ColorSchemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.DataContext is Models.ColorScheme scheme)
            {
                if (DataContext is ViewModels.SettingsViewModel viewModel)
                {
                    viewModel.SelectedColorScheme = scheme;
                }
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Changes are already applied through OnColorSchemeChanged event
            Close();
        }
    }
}
