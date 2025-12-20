using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ChessWPF.Models;
using ChessWPF.ViewModels;

namespace ChessWPF.Windows
{
    /// <summary>
    /// Interaction logic for GameSelectionWindow.xaml
    /// </summary>
    public partial class GameSelectionWindow : Window
    {
        public GameRecord SelectedGame { get; set; }

        public GameSelectionWindow(List<GameRecord> games)
        {
            InitializeComponent();
            DataContext = new GameSelectionViewModel(games, this);
        }
    }

    public class GameSelectionViewModel : NotifyPropertyChanged
    {
        private GameRecord selectedGame;
        private readonly List<GameRecord> games;
        private readonly Window window;

        public GameSelectionViewModel(List<GameRecord> games, Window window)
        {
            this.games = games;
            this.window = window;
            Games = games;
        }

        public List<GameRecord> Games { get; }

        public GameRecord SelectedGame
        {
            get => selectedGame;
            set
            {
                selectedGame = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadCommand => new RelayCommand(parameter =>
        {
            if (SelectedGame != null)
            {
                ((GameSelectionWindow)window).SelectedGame = SelectedGame;
                window.DialogResult = true;
                window.Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите партию для загрузки.", "Внимание", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        });

        public ICommand CancelCommand => new RelayCommand(parameter =>
        {
            window.DialogResult = false;
            window.Close();
        });
    }
}
