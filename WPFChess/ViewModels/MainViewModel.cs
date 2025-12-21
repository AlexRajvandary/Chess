using System;
using System.Windows;
using System.Windows.Media;
using ChessWPF.Models;
using ChessWPF.Services;

namespace ChessWPF.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private Brush darkSquareColor;
        private Brush lightSquareColor;
        private readonly SoundService soundService;
        private readonly GameStorageService gameStorageService;
        private SettingsViewModel settingsViewModel;
        private CapturedPiecesViewModel capturedPiecesViewModel;
        private GameStorageViewModel gameStorageViewModel;
        private GameViewModel gameViewModel;
        private HistoricalGamesViewModel historicalGamesViewModel;
        private MoveHistoryViewModel moveHistoryViewModel;
        private PanelManagementViewModel panelManagementViewModel;
        private TimerViewModel timerViewModel;
        
        public MainViewModel()
        {
            soundService = new SoundService();
            var gameService = new ChessGameService();
            gameStorageService = new GameStorageService();
            gameViewModel = new GameViewModel(gameService, soundService);
            timerViewModel = new TimerViewModel(gameViewModel.GameService, soundService);
            capturedPiecesViewModel = new CapturedPiecesViewModel(gameViewModel.GameService);
            gameViewModel.GetTimerViewModel = () => timerViewModel;
            gameViewModel.GetCapturedPiecesViewModel = () => capturedPiecesViewModel;
            gameViewModel.GetSettingsViewModel = () => settingsViewModel;
            gameViewModel.GetPanelManagementViewModel = () => panelManagementViewModel;
            LightSquareColor = new SolidColorBrush(Color.FromRgb(240, 217, 181));
            DarkSquareColor = new SolidColorBrush(Color.FromRgb(181, 136, 99));
            gameStorageViewModel = new GameStorageViewModel(gameViewModel.GameService, gameStorageService);
            gameStorageViewModel.OnGameLoadRequested = (gameRecord) => LoadGameFromRecord(gameRecord);
            OnPropertyChanged(nameof(GameStorage));
            settingsViewModel = new SettingsViewModel();
            settingsViewModel.OnColorSchemeChanged += (scheme) =>
            {
                LightSquareColor = scheme.LightSquareColor;
                DarkSquareColor = scheme.DarkSquareColor;
            };
            panelManagementViewModel = new PanelManagementViewModel();
            panelManagementViewModel.OnGamePanelOpened = () => gameStorageViewModel?.LoadSavedGames();
            settingsViewModel.OnPanelPositionChanged += (position) =>
            {
                panelManagementViewModel.UpdateSettingsPanelAlignment(position);
            };
            settingsViewModel.OnShowAvailableMovesChanged += (show) =>
            {
                if (!show)
                {
                    gameViewModel.ClearAvailableMoves();
                }
            };
            moveHistoryViewModel = new MoveHistoryViewModel(gameViewModel.GameService);
            moveHistoryViewModel.OnBoardUpdateRequired = () =>
            {
                gameViewModel.UpdateViewFromGameState();
                gameViewModel.Fen = gameViewModel.GameService.GetFen();
                gameViewModel.SetupBoard();
            };
            moveHistoryViewModel.OnCapturedPiecesUpdateRequired = () =>
            {
                capturedPiecesViewModel?.UpdateFromMoveHistory(gameViewModel.GetStateFromPiece);
            };
            moveHistoryViewModel.GetMoveHistoryString = () => gameViewModel.MoveHistory;
            moveHistoryViewModel.GetStateFromPiece = gameViewModel.GetStateFromPiece;
            gameViewModel.GetMoveHistoryViewModel = () => moveHistoryViewModel;
            OnPropertyChanged(nameof(MoveHistoryViewModel));
            gameStorageViewModel.LoadSavedGames();
            historicalGamesViewModel = new HistoricalGamesViewModel();
            historicalGamesViewModel.OnGameLoadRequested = (historicalGame) => LoadGameFromHistoricalGame(historicalGame);
            OnPropertyChanged(nameof(HistoricalGames));
            OnPropertyChanged(nameof(Game));
            historicalGamesViewModel.LoadHistoricalGames();
        }
        
        public Brush DarkSquareColor
        {
            get => darkSquareColor;
            set
            {
                darkSquareColor = value;
                OnPropertyChanged();
            }
        }
        
        public Brush LightSquareColor
        {
            get => lightSquareColor;
            set
            {
                lightSquareColor = value;
                OnPropertyChanged();
            }
        }
        
        public CapturedPiecesViewModel CapturedPieces
        {
            get => capturedPiecesViewModel;
            set
            {
                capturedPiecesViewModel = value;
                OnPropertyChanged();
            }
        }
        
        public GameViewModel Game
        {
            get => gameViewModel;
            set
            {
                gameViewModel = value;
                OnPropertyChanged();
            }
        }
        
        public GameStorageViewModel GameStorage
        {
            get => gameStorageViewModel;
            set
            {
                gameStorageViewModel = value;
                OnPropertyChanged();
            }
        }
        
        public HistoricalGamesViewModel HistoricalGames
        {
            get => historicalGamesViewModel;
            set
            {
                historicalGamesViewModel = value;
                OnPropertyChanged();
            }
        }
        
        public MoveHistoryViewModel MoveHistoryViewModel
        {
            get => moveHistoryViewModel;
            set
            {
                moveHistoryViewModel = value;
                OnPropertyChanged();
            }
        }
        
        public PanelManagementViewModel Panels
        {
            get => panelManagementViewModel;
            set
            {
                panelManagementViewModel = value;
                OnPropertyChanged();
            }
        }
        
        public SettingsViewModel SettingsViewModel
        {
            get => settingsViewModel;
            set
            {
                settingsViewModel = value;
                OnPropertyChanged();
            }
        }
        
        public TimerViewModel Timer
        {
            get => timerViewModel;
            set
            {
                timerViewModel = value;
                OnPropertyChanged();
            }
        }
            
        private void LoadGameFromHistoricalGame(HistoricalGame historicalGame)
        {
            try
            {
                timerViewModel?.ResetForLoadedGame();
                var moves = PgnService.ParsePgnMoves(historicalGame.PgnNotation);
                moveHistoryViewModel?.LoadGame(moves);
                historicalGamesViewModel.SelectedHistoricalGame = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке исторической партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadGameFromRecord(GameRecord gameRecord)
        {
            try
            {
                timerViewModel?.ResetForLoadedGame();
                var moves = PgnService.ParsePgnMoves(gameRecord.PgnNotation);
                moveHistoryViewModel?.LoadGame(moves);
                gameStorageViewModel.SelectedGame = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}