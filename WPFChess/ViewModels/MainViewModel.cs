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
        private readonly GameViewModel gameViewModel;
        private readonly TimerViewModel timerViewModel;
        private readonly CapturedPiecesViewModel capturedPiecesViewModel;
        private readonly GameStorageViewModel gameStorageViewModel;
        private readonly HistoricalGamesViewModel historicalGamesViewModel;
        private readonly MoveHistoryViewModel moveHistoryViewModel;
        private readonly PanelManagementViewModel panelManagementViewModel;
        private readonly SettingsViewModel settingsViewModel;
        public MainViewModel(
            GameViewModel gameViewModel,
            TimerViewModel timerViewModel,
            CapturedPiecesViewModel capturedPiecesViewModel,
            GameStorageViewModel gameStorageViewModel,
            HistoricalGamesViewModel historicalGamesViewModel,
            MoveHistoryViewModel moveHistoryViewModel,
            PanelManagementViewModel panelManagementViewModel,
            SettingsViewModel settingsViewModel)
        {
            this.gameViewModel = gameViewModel ?? throw new ArgumentNullException(nameof(gameViewModel));
            this.timerViewModel = timerViewModel ?? throw new ArgumentNullException(nameof(timerViewModel));
            this.capturedPiecesViewModel = capturedPiecesViewModel ?? throw new ArgumentNullException(nameof(capturedPiecesViewModel));
            this.gameStorageViewModel = gameStorageViewModel ?? throw new ArgumentNullException(nameof(gameStorageViewModel));
            this.historicalGamesViewModel = historicalGamesViewModel ?? throw new ArgumentNullException(nameof(historicalGamesViewModel));
            this.moveHistoryViewModel = moveHistoryViewModel ?? throw new ArgumentNullException(nameof(moveHistoryViewModel));
            this.panelManagementViewModel = panelManagementViewModel ?? throw new ArgumentNullException(nameof(panelManagementViewModel));
            this.settingsViewModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));

            Init();
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
        
        public CapturedPiecesViewModel CapturedPieces => capturedPiecesViewModel;
        public GameViewModel Game => gameViewModel;
        public GameStorageViewModel GameStorage => gameStorageViewModel;
        public HistoricalGamesViewModel HistoricalGames => historicalGamesViewModel;
        public MoveHistoryViewModel MoveHistoryViewModel => moveHistoryViewModel;
        public PanelManagementViewModel Panels => panelManagementViewModel;
        public SettingsViewModel SettingsViewModel => settingsViewModel;
        public TimerViewModel Timer => timerViewModel;
            
        private void LoadGameFromHistoricalGame(HistoricalGame historicalGame)
        {
            try
            {
                timerViewModel.ResetForLoadedGame();
                var moves = PgnService.ParsePgnMoves(historicalGame.PgnNotation);
                moveHistoryViewModel.LoadGame(moves);
                gameViewModel.SetHistoricalGameInfo(historicalGame);
                historicalGamesViewModel.SelectedHistoricalGame = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading historical game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadGameFromRecord(GameRecord gameRecord)
        {
            try
            {
                timerViewModel.ResetForLoadedGame();
                var moves = PgnService.ParsePgnMoves(gameRecord.PgnNotation);
                moveHistoryViewModel.LoadGame(moves);
                gameViewModel.SetHistoricalGameInfo(null);
                gameStorageViewModel.SelectedGame = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Init()
        {
            LightSquareColor = new SolidColorBrush(Color.FromRgb(240, 217, 181));
            DarkSquareColor = new SolidColorBrush(Color.FromRgb(181, 136, 99));

            gameStorageViewModel.OnGameLoadRequested = (gameRecord) => LoadGameFromRecord(gameRecord);

            settingsViewModel.OnColorSchemeChanged += (scheme) =>
            {
                LightSquareColor = scheme.LightSquareColor;
                DarkSquareColor = scheme.DarkSquareColor;
            };

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
            moveHistoryViewModel.OnBoardUpdateRequired = () =>
            {
                gameViewModel.UpdateViewFromGameState();
                gameViewModel.Fen = gameViewModel.GameService.GetFen();
                gameViewModel.SetupBoard();
            };
            moveHistoryViewModel.OnCapturedPiecesUpdateRequired = () =>
            {
                capturedPiecesViewModel.UpdateFromMoveHistory(gameViewModel.GetStateFromPiece);
            };
            moveHistoryViewModel.GetMoveHistoryString = () => gameViewModel.MoveHistory;
            moveHistoryViewModel.GetStateFromPiece = gameViewModel.GetStateFromPiece;
            gameViewModel.OnGameStateUpdated = () => OnPropertyChanged(nameof(Game));
            gameStorageViewModel.LoadSavedGames();
            historicalGamesViewModel.OnGameLoadRequested = (historicalGame) => LoadGameFromHistoricalGame(historicalGame);
            historicalGamesViewModel.LoadHistoricalGames();
        }
    }
}