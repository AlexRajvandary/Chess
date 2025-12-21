using System.Windows;
using System.Windows.Controls;
using ChessWPF.ViewModels;
using ChessWPF.Models;

namespace ChessWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void SettingsOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Close settings panel when clicking on overlay (but not on the panel itself)
            if (e.OriginalSource == sender && DataContext is MainViewModel viewModel)
            {
                viewModel.Panels.IsSettingsPanelVisible = false;
            }
        }

        private void SettingsPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Prevent closing when clicking inside the panel
            e.Handled = true;
        }

        private void ColorSchemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.DataContext is ColorScheme scheme)
            {
                if (DataContext is MainViewModel mainViewModel && mainViewModel.SettingsViewModel != null)
                {
                    mainViewModel.SettingsViewModel.SelectedColorScheme = scheme;
                }
            }
        }

        private void GameOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Close game panel when clicking on overlay (but not on the panel itself)
            if (e.OriginalSource == sender && DataContext is MainViewModel viewModel)
            {
                viewModel.Panels.IsGamePanelVisible = false;
            }
        }

        private void GamePanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Prevent closing when clicking inside the panel
            e.Handled = true;
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Close game panel after starting new game
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.Panels.IsGamePanelVisible = false;
            }
        }

        private void AboutOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Close about panel when clicking on overlay (but not on the panel itself)
            if (e.OriginalSource == sender && DataContext is MainViewModel viewModel)
            {
                viewModel.Panels.IsAboutPanelVisible = false;
            }
        }

        private void AboutPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Prevent closing when clicking inside the panel
            e.Handled = true;
        }
    }
}