using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ChessWPF.Models;
using ChessWPF.Services;

namespace ChessWPF.ViewModels
{
    public class GameStorageViewModel : NotifyPropertyChanged
    {
        private readonly ChessGameService gameService;
        private readonly GameStorageService gameStorageService;
        private ICommand loadSelectedGameCommand;
        private ICommand refreshGamesCommand;
        private ICommand saveGameCommand;
        private ObservableCollection<GameRecord> savedGames;
        private GameRecord selectedGame;

        public GameStorageViewModel(ChessGameService gameService, GameStorageService gameStorageService)
        {
            this.gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            this.gameStorageService = gameStorageService ?? throw new ArgumentNullException(nameof(gameStorageService));
            SavedGames = new ObservableCollection<GameRecord>();
        }

        public Action<GameRecord> OnGameLoadRequested { get; set; }
        
        public ICommand LoadSelectedGameCommand => loadSelectedGameCommand ??= new RelayCommand(parameter =>
        {
            if (SelectedGame != null)
            {
                OnGameLoadRequested?.Invoke(SelectedGame);
            }
        }, parameter => SelectedGame != null);

        public ICommand RefreshGamesCommand => refreshGamesCommand ??= new RelayCommand(parameter =>
        {
            LoadSavedGames();
        });

        public ICommand SaveGameCommand => saveGameCommand ??= new RelayCommand(parameter =>
        {
            try
            {
                var gameRecord = gameStorageService.SaveGame(
                    gameService.CurrentGame,
                    "White",
                    "Black",
                    "Casual Game",
                    "Local",
                    "1"
                );
                if (gameRecord != null)
                {
                    MessageBox.Show($"Партия успешно сохранена!\nID: {gameRecord.Id}\nХодов: {gameRecord.MoveCount}", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSavedGames();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        public ObservableCollection<GameRecord> SavedGames
        {
            get => savedGames;
            set
            {
                savedGames = value;
                OnPropertyChanged();
            }
        }

        public GameRecord SelectedGame
        {
            get => selectedGame;
            set
            {
                selectedGame = value;
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }

        }

        public void LoadSavedGames()
        {
            try
            {
                var games = GameStorageService.GetAllGames();
                SavedGames.Clear();
                foreach (var game in games)
                {
                    SavedGames.Add(game);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сохраненных партий: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}