using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ChessLib;
using ChessWPF.Models;
using ChessWPF.Services;

namespace ChessWPF.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private const string pathOfFenFile = "fen.txt";

        private BoardViewModel board;
        private ICommand cellCommand;
        
        private ChessGameService gameService;
        private ObservableCollection<string> moves;
        private ICommand newGameCommand;
        private ObservableCollection<string> playerMoves;
        private string fen;
        private string moveHistory;
        private bool showFenNotation = true;
        private ICommand toggleNotationCommand;
        private ICommand openAboutCommand;
        private ICommand openSettingsCommand;
        private ICommand closeSettingsCommand;
        private ICommand openGameCommand;
        private ICommand closeGameCommand;
        private ICommand saveGameCommand;
        private ICommand closeAboutCommand;
        private ICommand toggleSidePanelCommand;
        private Brush lightSquareColor;
        private Brush darkSquareColor;
        private readonly SoundService soundService;
        private readonly GameStorageService gameStorageService;
        private SettingsViewModel settingsViewModel;
        private bool isSettingsPanelVisible;
        private bool isGamePanelVisible;
        private bool isAboutPanelVisible;
        private bool isSidePanelVisible = true;
        private ObservableCollection<GameRecord> savedGames;
        private GameRecord selectedGame;
        private ICommand loadSelectedGameCommand;
        private ICommand refreshGamesCommand;
        private ObservableCollection<HistoricalGame> historicalGames;
        private HistoricalGame selectedHistoricalGame;
        private ICommand loadSelectedHistoricalGameCommand;
        private ICommand refreshHistoricalGamesCommand;
        private ICommand importPgnFileCommand;
        private bool isImporting = false;
        private int historicalGamesCurrentPage = 1;
        private int historicalGamesPageSize = 7;
        private int historicalGamesTotalCount = 0;
        private long historicalGamesDatabaseSize = 0;
        private ICommand previousHistoricalGamesPageCommand;
        private ICommand nextHistoricalGamesPageCommand;
        private int importProgress = 0;
        private string importProgressText = "";
        private string importResultMessage = "";
        private System.Windows.Media.Brush importResultColor = System.Windows.Media.Brushes.Black;
        private ObservableCollection<CapturedPieceInfo> capturedByWhite;
        private ObservableCollection<CapturedPieceInfo> capturedByBlack;
        private bool isGameLoaded = false;
        private List<string> loadedGameMoves = new List<string>();
        private int selectedMoveIndex = -1;
        private ObservableCollection<MoveDisplayItem> moveHistoryItems;
        private ICommand previousMoveCommand;
        private ICommand nextMoveCommand;
        private ICommand toggleAutoPlayCommand;
        private System.Windows.Threading.DispatcherTimer autoPlayTimer;
        private bool isAutoPlaying = false;
        private System.Windows.HorizontalAlignment settingsPanelAlignment = System.Windows.HorizontalAlignment.Left;
        private System.Windows.HorizontalAlignment gamePanelAlignment = System.Windows.HorizontalAlignment.Left;
        private System.Windows.HorizontalAlignment aboutPanelAlignment = System.Windows.HorizontalAlignment.Left;
        
        // Timer properties
        private TimeSpan whitePlayerTime = TimeSpan.Zero;
        private TimeSpan blackPlayerTime = TimeSpan.Zero;
        private TimeSpan initialTimePerPlayer = TimeSpan.FromMinutes(10); // Default 10 minutes
        private System.Windows.Threading.DispatcherTimer gameTimer;
        private bool isWhitePlayerActive = true;
        private bool isTimerEnabled = false; // Setting for next game
        private Models.TimeOption selectedTimeOption;
        private bool isFirstMove = true; // Track if this is the first move of the game
        private bool currentGameHasTimers = false; // Whether current game has timers enabled

        public MainViewModel()
        {
            Board = new BoardViewModel();
            gameService = new ChessGameService();
            // Initialize with default colors
            LightSquareColor = new SolidColorBrush(Color.FromRgb(240, 217, 181)); // Bisque
            DarkSquareColor = new SolidColorBrush(Color.FromRgb(181, 136, 99));   // SandyBrown
            
            // Initialize sound service
            soundService = new SoundService();
            
            // Initialize game storage service
            gameStorageService = new GameStorageService();
            
            // Initialize settings view model
            settingsViewModel = new SettingsViewModel();
            settingsViewModel.OnColorSchemeChanged += (scheme) =>
            {
                LightSquareColor = scheme.LightSquareColor;
                DarkSquareColor = scheme.DarkSquareColor;
            };
            settingsViewModel.OnPanelPositionChanged += (position) =>
            {
                SettingsPanelAlignment = position == ViewModels.PanelPosition.Left 
                    ? System.Windows.HorizontalAlignment.Left 
                    : System.Windows.HorizontalAlignment.Right;
            };
            settingsViewModel.OnShowAvailableMovesChanged += (show) =>
            {
                // If disabled, clear current highlights
                if (!show)
                {
                    ClearAvailableMoves();
                }
            };

            // Initialize saved games list
            SavedGames = new ObservableCollection<GameRecord>();
            LoadSavedGames();
            
            // Initialize historical games list
            HistoricalGames = new ObservableCollection<HistoricalGame>();
            LoadHistoricalGames();
            
            // Initialize game timer
            gameTimer = new System.Windows.Threading.DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += GameTimer_Tick;
            
            // Initialize time option (default 10 minutes)
            SelectedTimeOption = new Models.TimeOption { Time = TimeSpan.FromMinutes(10), Display = "10 минут" };

            // Initialize captured pieces collections
            CapturedByWhite = new ObservableCollection<CapturedPieceInfo>();
            CapturedByBlack = new ObservableCollection<CapturedPieceInfo>();
            
            // Initialize move history items
            MoveHistoryItems = new ObservableCollection<MoveDisplayItem>();
            
            // Initialize auto-play timer
            autoPlayTimer = new System.Windows.Threading.DispatcherTimer();
            autoPlayTimer.Interval = TimeSpan.FromSeconds(1.0); // 1 second per move
            autoPlayTimer.Tick += AutoPlayTimer_Tick;
        }

        private void AutoPlayTimer_Tick(object sender, EventArgs e)
        {
            if (IsGameLoaded && SelectedMoveIndex < loadedGameMoves.Count - 1)
            {
                NavigateToMove(SelectedMoveIndex + 1);
            }
            else
            {
                // Reached the end, stop auto-play
                StopAutoPlay();
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

        public Brush DarkSquareColor
        {
            get => darkSquareColor;
            set
            {
                darkSquareColor = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenAboutCommand => openAboutCommand ??= new RelayCommand(parameter =>
        {
            IsAboutPanelVisible = true;
        });

        public ICommand CloseAboutCommand => closeAboutCommand ??= new RelayCommand(parameter =>
        {
            IsAboutPanelVisible = false;
        });

        public ICommand OpenSettingsCommand => openSettingsCommand ??= new RelayCommand(parameter =>
        {
            IsSettingsPanelVisible = true;
        });

        public ICommand CloseSettingsCommand => closeSettingsCommand ??= new RelayCommand(parameter =>
        {
            IsSettingsPanelVisible = false;
        });

        public ICommand OpenGameCommand => openGameCommand ??= new RelayCommand(parameter =>
        {
            IsGamePanelVisible = true;
            // Refresh saved games list when opening the panel
            LoadSavedGames();
        });

        public ICommand CloseGameCommand => closeGameCommand ??= new RelayCommand(parameter =>
        {
            IsGamePanelVisible = false;
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
                    
                    // Refresh saved games list
                    LoadSavedGames();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        /// <summary>
        /// Collection of saved games
        /// </summary>
        public ObservableCollection<GameRecord> SavedGames
        {
            get => savedGames;
            set
            {
                savedGames = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Selected game from the list
        /// </summary>
        public GameRecord SelectedGame
        {
            get => selectedGame;
            set
            {
                selectedGame = value;
                OnPropertyChanged();
                // Force command to re-evaluate CanExecute
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Command to load selected game
        /// </summary>
        public ICommand LoadSelectedGameCommand => loadSelectedGameCommand ??= new RelayCommand(parameter =>
        {
            if (SelectedGame != null)
            {
                LoadGameFromRecord(SelectedGame);
            }
        }, parameter => SelectedGame != null);

        public ICommand LoadSelectedHistoricalGameCommand => loadSelectedHistoricalGameCommand ??= new RelayCommand(parameter =>
        {
            if (SelectedHistoricalGame != null)
            {
                LoadGameFromHistoricalGame(SelectedHistoricalGame);
            }
        }, parameter => SelectedHistoricalGame != null);

        public ICommand RefreshHistoricalGamesCommand => refreshHistoricalGamesCommand ??= new RelayCommand(parameter =>
        {
            HistoricalGamesCurrentPage = 1;
            LoadHistoricalGames();
        });

        public ICommand ImportPgnFileCommand => importPgnFileCommand ??= new RelayCommand(parameter =>
        {
            ImportPgnFile();
        }, parameter => !IsImporting);

        public bool IsImporting
        {
            get => isImporting;
            set
            {
                isImporting = value;
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public int ImportProgress
        {
            get => importProgress;
            set
            {
                importProgress = value;
                OnPropertyChanged();
            }
        }

        public string ImportProgressText
        {
            get => importProgressText;
            set
            {
                importProgressText = value;
                OnPropertyChanged();
            }
        }

        public string ImportResultMessage
        {
            get => importResultMessage;
            set
            {
                importResultMessage = value;
                OnPropertyChanged();
            }
        }

        public System.Windows.Media.Brush ImportResultColor
        {
            get => importResultColor;
            set
            {
                importResultColor = value;
                OnPropertyChanged();
            }
        }

        private void ImportPgnFile()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "PGN files (*.pgn)|*.pgn|All files (*.*)|*.*",
                    Title = "Select PGN file to import"
                };

                // Try to set initial directory to Assets/HistoricalGames
                var assetsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "HistoricalGames");
                if (System.IO.Directory.Exists(assetsPath))
                {
                    dialog.InitialDirectory = assetsPath;
                }

                if (dialog.ShowDialog() == true)
                {
                    var filePath = dialog.FileName;
                    var fileName = System.IO.Path.GetFileName(filePath);

                    // Check if file already parsed
                    if (GameStorageService.IsFileParsed(fileName))
                    {
                        ImportResultMessage = $"Файл {fileName} уже был импортирован ранее.";
                        ImportResultColor = System.Windows.Media.Brushes.Orange;
                        return;
                    }

                    IsImporting = true;
                    ImportProgress = 0;
                    ImportProgressText = "Начинаем импорт...";
                    ImportResultMessage = "";

                    // Run import on background thread
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            var result = GameStorageService.ImportPgnFile(filePath, (progress, current, total) =>
                            {
                                // Update progress on UI thread
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ImportProgress = progress;
                                    if (progress == 0 && current == 0)
                                    {
                                        // Initial callback - file is being parsed
                                        ImportProgressText = "Анализ файла...";
                                    }
                                    else if (total > 0)
                                    {
                                        ImportProgressText = $"Обработано игр: {current} из {total} ({progress}%)";
                                    }
                                    else
                                    {
                                        ImportProgressText = $"Обработано игр: {current} ({progress}%)";
                                    }
                                });
                            });

                            // Update UI on main thread
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                IsImporting = false;
                                ImportProgress = 100;

                                if (result.Success)
                                {
                                    ImportResultMessage = result.Message;
                                    ImportResultColor = System.Windows.Media.Brushes.Green;
                                    // Refresh games list and reset to first page
                                    HistoricalGamesCurrentPage = 1;
                                    LoadHistoricalGames();
                                }
                                else
                                {
                                    ImportResultMessage = result.Message;
                                    ImportResultColor = System.Windows.Media.Brushes.Red;
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                IsImporting = false;
                                ImportResultMessage = $"Ошибка при импорте: {ex.Message}";
                                ImportResultColor = System.Windows.Media.Brushes.Red;
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                IsImporting = false;
                ImportResultMessage = $"Ошибка: {ex.Message}";
                ImportResultColor = System.Windows.Media.Brushes.Red;
            }
        }

        public ObservableCollection<HistoricalGame> HistoricalGames
        {
            get => historicalGames;
            set
            {
                historicalGames = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Current page number for historical games pagination
        /// </summary>
        public int HistoricalGamesCurrentPage
        {
            get => historicalGamesCurrentPage;
            set
            {
                historicalGamesCurrentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HistoricalGamesPageInfo));
                OnPropertyChanged(nameof(CanGoToPreviousHistoricalGamesPage));
                OnPropertyChanged(nameof(CanGoToNextHistoricalGamesPage));
            }
        }

        /// <summary>
        /// Total count of historical games
        /// </summary>
        public int HistoricalGamesTotalCount
        {
            get => historicalGamesTotalCount;
            set
            {
                historicalGamesTotalCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HistoricalGamesPageInfo));
                OnPropertyChanged(nameof(HistoricalGamesCountInfo));
                OnPropertyChanged(nameof(CanGoToPreviousHistoricalGamesPage));
                OnPropertyChanged(nameof(CanGoToNextHistoricalGamesPage));
            }
        }

        /// <summary>
        /// Database size in bytes
        /// </summary>
        public long HistoricalGamesDatabaseSize
        {
            get => historicalGamesDatabaseSize;
            set
            {
                historicalGamesDatabaseSize = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HistoricalGamesDatabaseSizeFormatted));
                OnPropertyChanged(nameof(HistoricalGamesCountInfo));
            }
        }

        /// <summary>
        /// Formatted database size (KB, MB, etc.)
        /// </summary>
        public string HistoricalGamesDatabaseSizeFormatted
        {
            get
            {
                if (historicalGamesDatabaseSize == 0)
                    return "0 КБ";
                
                double size = historicalGamesDatabaseSize;
                string[] units = { "Б", "КБ", "МБ", "ГБ" };
                int unitIndex = 0;
                
                while (size >= 1024 && unitIndex < units.Length - 1)
                {
                    size /= 1024;
                    unitIndex++;
                }
                
                return $"{size:F2} {units[unitIndex]}";
            }
        }

        /// <summary>
        /// Combined info: total count and database size
        /// </summary>
        public string HistoricalGamesCountInfo
        {
            get
            {
                if (historicalGamesTotalCount == 0)
                    return "Нет партий";
                
                return $"{historicalGamesTotalCount} партий ({HistoricalGamesDatabaseSizeFormatted})";
            }
        }

        /// <summary>
        /// Page size for historical games pagination
        /// </summary>
        public int HistoricalGamesPageSize
        {
            get => historicalGamesPageSize;
            set
            {
                historicalGamesPageSize = value;
                OnPropertyChanged();
                LoadHistoricalGames();
            }
        }

        /// <summary>
        /// Page info string for display
        /// </summary>
        public string HistoricalGamesPageInfo
        {
            get
            {
                if (historicalGamesTotalCount == 0)
                    return "Нет партий";
                
                int totalPages = (int)Math.Ceiling((double)historicalGamesTotalCount / historicalGamesPageSize);
                int startIndex = (historicalGamesCurrentPage - 1) * historicalGamesPageSize + 1;
                int endIndex = Math.Min(historicalGamesCurrentPage * historicalGamesPageSize, historicalGamesTotalCount);
                
                return $"Страница {historicalGamesCurrentPage} из {totalPages} ({startIndex}-{endIndex} из {historicalGamesTotalCount})";
            }
        }

        /// <summary>
        /// Can navigate to previous page
        /// </summary>
        public bool CanGoToPreviousHistoricalGamesPage => historicalGamesCurrentPage > 1;

        /// <summary>
        /// Can navigate to next page
        /// </summary>
        public bool CanGoToNextHistoricalGamesPage
        {
            get
            {
                int totalPages = (int)Math.Ceiling((double)historicalGamesTotalCount / historicalGamesPageSize);
                return historicalGamesCurrentPage < totalPages;
            }
        }

        /// <summary>
        /// Command to go to previous page
        /// </summary>
        public ICommand PreviousHistoricalGamesPageCommand => previousHistoricalGamesPageCommand ??= new RelayCommand(parameter =>
        {
            if (CanGoToPreviousHistoricalGamesPage)
            {
                HistoricalGamesCurrentPage--;
                LoadHistoricalGames();
            }
        }, parameter => CanGoToPreviousHistoricalGamesPage);

        /// <summary>
        /// Command to go to next page
        /// </summary>
        public ICommand NextHistoricalGamesPageCommand => nextHistoricalGamesPageCommand ??= new RelayCommand(parameter =>
        {
            if (CanGoToNextHistoricalGamesPage)
            {
                HistoricalGamesCurrentPage++;
                LoadHistoricalGames();
            }
        }, parameter => CanGoToNextHistoricalGamesPage);

        public HistoricalGame SelectedHistoricalGame
        {
            get => selectedHistoricalGame;
            set
            {
                selectedHistoricalGame = value;
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Command to refresh saved games list
        /// </summary>
        public ICommand RefreshGamesCommand => refreshGamesCommand ??= new RelayCommand(parameter =>
        {
            LoadSavedGames();
        });

        /// <summary>
        /// Loads saved games from database
        /// </summary>
        private void LoadSavedGames()
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
                MessageBox.Show($"Ошибка при загрузке списка партий: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadHistoricalGames()
        {
            try
            {
                var (games, totalCount) = GameStorageService.GetHistoricalGamesPaginated(historicalGamesCurrentPage, historicalGamesPageSize);
                HistoricalGamesTotalCount = totalCount;
                HistoricalGamesDatabaseSize = GameStorageService.GetDatabaseSize();
                HistoricalGames.Clear();
                foreach (var game in games)
                {
                    HistoricalGames.Add(game);
                }
                OnPropertyChanged(nameof(HistoricalGamesPageInfo));
                OnPropertyChanged(nameof(HistoricalGamesDatabaseSizeFormatted));
                OnPropertyChanged(nameof(CanGoToPreviousHistoricalGamesPage));
                OnPropertyChanged(nameof(CanGoToNextHistoricalGamesPage));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке исторических партий: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads a game from HistoricalGame
        /// </summary>
        private void LoadGameFromHistoricalGame(HistoricalGame historicalGame)
        {
            try
            {
                // Loaded games don't have timers
                currentGameHasTimers = false;
                OnPropertyChanged(nameof(ShowTimers));
                
                // Parse PGN and save moves
                loadedGameMoves = PgnService.ParsePgnMoves(historicalGame.PgnNotation);
                IsGameLoaded = true;
                
                // Apply all moves to show final position
                NavigateToMove(loadedGameMoves.Count - 1);
                
                // Clear selection after loading
                SelectedHistoricalGame = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке исторической партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads a game from GameRecord
        /// </summary>
        private void LoadGameFromRecord(GameRecord gameRecord)
        {
            try
            {
                // Loaded games don't have timers
                currentGameHasTimers = false;
                OnPropertyChanged(nameof(ShowTimers));
                
                // Parse PGN and save moves
                loadedGameMoves = PgnService.ParsePgnMoves(gameRecord.PgnNotation);
                IsGameLoaded = true;
                
                // Apply all moves to show final position
                NavigateToMove(loadedGameMoves.Count - 1);
                
                // Clear selection after loading
                SelectedGame = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Navigates to a specific move in the loaded game
        /// </summary>
        private void NavigateToMove(int moveIndex)
        {
            if (!IsGameLoaded || moveIndex < -1 || moveIndex >= loadedGameMoves.Count)
                return;

            // Start new game
            gameService.StartNewGame();

            // Apply moves up to the selected index
            for (int i = 0; i <= moveIndex; i++)
            {
                var moveNotation = loadedGameMoves[i];
                var moveInfo = PgnMoveParser.ParseMove(moveNotation, gameService.CurrentGame);
                if (moveInfo != null)
                {
                    var result = gameService.MakeMove(moveInfo.From, moveInfo.To);
                    // Continue even if move fails
                }
            }

            SelectedMoveIndex = moveIndex;

            // Update view
            UpdateViewFromGameState();
            UpdateCapturedPieces();
            
            // Update move history display
            UpdateMoveHistoryItems();
            
            Fen = gameService.GetFen();
            SetupBoard();
            
            // Force command re-evaluation
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Updates the move history items collection for clickable display
        /// </summary>
        private void UpdateMoveHistoryItems()
        {
            var oldCount = MoveHistoryItems.Count;
            MoveHistoryItems.Clear();
            
            if (IsGameLoaded && loadedGameMoves != null && loadedGameMoves.Count > 0)
            {
                // Parse moves and create items - group white and black moves together
                int moveNumber = 1;
                for (int i = 0; i < loadedGameMoves.Count; i += 2)
                {
                    int whiteIndex = i;
                    int blackIndex = i + 1;
                    string whiteMove = loadedGameMoves[whiteIndex];
                    string blackMove = blackIndex < loadedGameMoves.Count ? loadedGameMoves[blackIndex] : null;
                    
                    var item = new MoveDisplayItem
                    {
                        MoveNumber = moveNumber,
                        WhiteMoveIndex = whiteIndex,
                        WhiteMoveText = whiteMove,
                        IsWhiteSelected = whiteIndex == SelectedMoveIndex,
                        SelectWhiteMoveCommand = new RelayCommand(_ => SelectMove(whiteIndex))
                    };
                    
                    if (blackMove != null)
                    {
                        item.BlackMoveIndex = blackIndex;
                        item.BlackMoveText = blackMove;
                        item.IsBlackSelected = blackIndex == SelectedMoveIndex;
                        item.SelectBlackMoveCommand = new RelayCommand(_ => SelectMove(blackIndex));
                    }
                    
                    MoveHistoryItems.Add(item);
                    moveNumber++;
                }
                
                // Update selection state for all items
                foreach (var item in MoveHistoryItems)
                {
                    item.IsWhiteSelected = item.WhiteMoveIndex == SelectedMoveIndex;
                    if (item.BlackMoveText != null)
                    {
                        item.IsBlackSelected = item.BlackMoveIndex == SelectedMoveIndex;
                    }
                }
            }
            else
            {
                // For non-loaded games, parse MoveHistory string (format: "1. e4 e5 2. Nf3 Nc6...")
                if (!string.IsNullOrEmpty(MoveHistory))
                {
                    ParseMoveHistoryString(MoveHistory);
                }
            }
            
            // Force UI update if collection changed
            if (MoveHistoryItems.Count != oldCount)
            {
                OnPropertyChanged(nameof(MoveHistoryItems));
            }
        }

        /// <summary>
        /// Parses move history string and creates display items
        /// Format: "1. e4 e5 2. Nf3 Nc6..." where each number followed by white and optionally black move
        /// Note: Removes move numbers from move text since we have a separate column for move numbers
        /// </summary>
        private void ParseMoveHistoryString(string moveHistory)
        {
            if (string.IsNullOrWhiteSpace(moveHistory))
                return;

            // Remove newlines and normalize whitespace - replace all whitespace with single space
            moveHistory = System.Text.RegularExpressions.Regex.Replace(moveHistory, @"\s+", " ").Trim();

            // Pattern: "1. e4 e5" or "1.e4 e5" or "1. e4" (if only white move)
            // Matches: number, dot, optional space, then white move (non-whitespace, non-digit), optionally black move
            // This pattern handles moves like "e4", "Nf3", "O-O", "e8=Q", etc.
            // Note: explicitly excludes digits to avoid matching move numbers in the move text
            var regex = new System.Text.RegularExpressions.Regex(@"(\d+)\.\s*([^\s\d]+(?:\+|\#)?)(?:\s+([^\s\d]+(?:\+|\#)?))?");
            var matches = regex.Matches(moveHistory);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Success && match.Groups.Count >= 3)
                {
                    int moveNumber = int.Parse(match.Groups[1].Value);
                    string whiteMove = match.Groups[2].Value.TrimEnd('+', '#');
                    string blackMove = match.Groups.Count > 3 && match.Groups[3].Success && !string.IsNullOrWhiteSpace(match.Groups[3].Value)
                        ? match.Groups[3].Value.TrimEnd('+', '#')
                        : null;

                    // Only add if we have at least a white move
                    if (!string.IsNullOrWhiteSpace(whiteMove))
                    {
                        var item = new MoveDisplayItem
                        {
                            MoveNumber = moveNumber,
                            WhiteMoveText = whiteMove,
                            BlackMoveText = blackMove
                        };

                        MoveHistoryItems.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Selects a move from the loaded game
        /// </summary>
        public void SelectMove(int moveIndex)
        {
            NavigateToMove(moveIndex);
        }

        /// <summary>
        /// Indicates if a game is currently loaded
        /// </summary>
        public bool IsGameLoaded
        {
            get => isGameLoaded;
            set
            {
                isGameLoaded = value;
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Currently selected move index in loaded game
        /// </summary>
        public int SelectedMoveIndex
        {
            get => selectedMoveIndex;
            set
            {
                selectedMoveIndex = value;
                OnPropertyChanged();
                UpdateMoveHistoryItems();
            }
        }

        /// <summary>
        /// Collection of move history items for clickable display
        /// </summary>
        public ObservableCollection<MoveDisplayItem> MoveHistoryItems
        {
            get => moveHistoryItems;
            set
            {
                moveHistoryItems = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Command to navigate to previous move
        /// </summary>
        public ICommand PreviousMoveCommand => previousMoveCommand ??= new RelayCommand(
            _ =>
            {
                if (IsGameLoaded && SelectedMoveIndex > -1)
                {
                    NavigateToMove(SelectedMoveIndex - 1);
                }
            },
            _ => IsGameLoaded && SelectedMoveIndex > -1);

        /// <summary>
        /// Command to navigate to next move
        /// </summary>
        public ICommand NextMoveCommand => nextMoveCommand ??= new RelayCommand(
            _ =>
            {
                if (IsGameLoaded && SelectedMoveIndex < loadedGameMoves.Count - 1)
                {
                    NavigateToMove(SelectedMoveIndex + 1);
                }
            },
            _ => IsGameLoaded && SelectedMoveIndex < loadedGameMoves.Count - 1);

        /// <summary>
        /// Command to toggle auto-play
        /// </summary>
        public ICommand ToggleAutoPlayCommand => toggleAutoPlayCommand ??= new RelayCommand(
            _ =>
            {
                if (isAutoPlaying)
                {
                    StopAutoPlay();
                }
                else
                {
                    StartAutoPlay();
                }
            },
            _ => IsGameLoaded);

        /// <summary>
        /// Indicates if auto-play is currently active
        /// </summary>
        public bool IsAutoPlaying
        {
            get => isAutoPlaying;
            private set
            {
                isAutoPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AutoPlayButtonText));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Text for auto-play button
        /// </summary>
        public string AutoPlayButtonText => isAutoPlaying ? "⏸ Pause" : "▶ Play";

        /// <summary>
        /// Starts auto-play
        /// </summary>
        private void StartAutoPlay()
        {
            if (!IsGameLoaded)
                return;

            IsAutoPlaying = true;
            autoPlayTimer.Start();
        }

        /// <summary>
        /// Stops auto-play
        /// </summary>
        private void StopAutoPlay()
        {
            IsAutoPlaying = false;
            autoPlayTimer.Stop();
        }

        /// <summary>
        /// Formats moves from PGN list for display
        /// </summary>
        private string FormatMovesFromPgn(List<string> moves)
        {
            if (moves == null || moves.Count == 0)
                return string.Empty;

            var result = new System.Text.StringBuilder();
            int moveNumber = 1;
            
            for (int i = 0; i < moves.Count; i += 2)
            {
                result.Append($"{moveNumber}. ");
                
                // White move
                if (i < moves.Count)
                {
                    result.Append(moves[i]);
                }
                
                // Black move
                if (i + 1 < moves.Count)
                {
                    result.Append($" {moves[i + 1]}");
                }
                
                result.Append(" ");
                moveNumber++;
            }
            
            return result.ToString().Trim();
        }

        public ICommand ToggleSidePanelCommand => toggleSidePanelCommand ??= new RelayCommand(parameter =>
        {
            IsSidePanelVisible = !IsSidePanelVisible;
        });

        public SettingsViewModel SettingsViewModel
        {
            get => settingsViewModel;
            set
            {
                settingsViewModel = value;
                OnPropertyChanged();
            }
        }

        public bool IsSettingsPanelVisible
        {
            get => isSettingsPanelVisible;
            set
            {
                isSettingsPanelVisible = value;
                OnPropertyChanged();
            }
        }

        public System.Windows.HorizontalAlignment SettingsPanelAlignment
        {
            get => settingsPanelAlignment;
            set
            {
                settingsPanelAlignment = value;
                OnPropertyChanged();
            }
        }

        public bool IsGamePanelVisible
        {
            get => isGamePanelVisible;
            set
            {
                isGamePanelVisible = value;
                OnPropertyChanged();
            }
        }

        public System.Windows.HorizontalAlignment GamePanelAlignment
        {
            get => gamePanelAlignment;
            set
            {
                gamePanelAlignment = value;
                OnPropertyChanged();
            }
        }

        public bool IsAboutPanelVisible
        {
            get => isAboutPanelVisible;
            set
            {
                isAboutPanelVisible = value;
                OnPropertyChanged();
            }
        }

        public System.Windows.HorizontalAlignment AboutPanelAlignment
        {
            get => aboutPanelAlignment;
            set
            {
                aboutPanelAlignment = value;
                OnPropertyChanged();
            }
        }

        public bool IsSidePanelVisible
        {
            get => isSidePanelVisible;
            set
            {
                isSidePanelVisible = value;
                OnPropertyChanged();
            }
        }

        public BoardViewModel Board
        {
            get => board;
            set
            {
                board = value;
                OnPropertyChanged();
            }
        }

        public ICommand CellCommand => cellCommand ??= new RelayCommand(parameter =>
        {
            CellViewModel currentCell = (CellViewModel)parameter;
            CellViewModel previousActiveCell = Board.FirstOrDefault(x => x.Active);

            var currentPos = new ChessLib.Position(currentCell.Position.Horizontal, currentCell.Position.Vertical);
            
            if (previousActiveCell == null)
            {
                // If clicking on empty cell when nothing is selected, do nothing
                if (currentCell.State == CellUIState.Empty)
                {
                    return;
                }
                SelectPiece(currentCell);
            }
            else if (previousActiveCell == currentCell)
            {
                // Clicking on the same cell - deselect
                previousActiveCell.Active = false;
                ClearAvailableMoves();
            }
            else
            {
                // Check if game ended due to time expiration
                if (currentGameHasTimers && !gameTimer.IsEnabled)
                {
                    // Check if any player's time is 0
                    if (whitePlayerTime.TotalSeconds <= 0 || blackPlayerTime.TotalSeconds <= 0)
                    {
                        MessageBox.Show("Игра завершена из-за истечения времени. Начните новую игру.", "Игра завершена", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                
                var fromPos = new ChessLib.Position(previousActiveCell.Position.Horizontal, previousActiveCell.Position.Vertical);
                var result = gameService.MakeMove(fromPos, currentPos);
                
                if (result.IsValid)
                {
                    soundService.PlayMoveSound(result);
                    
                    // Update captured pieces if a piece was captured
                    if (result.CapturedPiece != null)
                    {
                        var capturedState = GetStateFromPiece(result.CapturedPiece);
                        // Determine the color of the capturing piece from the previous cell
                        // The piece that made the move was in previousActiveCell
                        bool isWhiteCapturing = previousActiveCell.State == CellUIState.WhitePawn ||
                                               previousActiveCell.State == CellUIState.WhiteRook ||
                                               previousActiveCell.State == CellUIState.WhiteKnight ||
                                               previousActiveCell.State == CellUIState.WhiteBishop ||
                                               previousActiveCell.State == CellUIState.WhiteQueen ||
                                               previousActiveCell.State == CellUIState.WhiteKing;
                        
                        // Add to the collection of the player who captured it
                        var targetCollection = isWhiteCapturing ? CapturedByWhite : CapturedByBlack;
                        var existingPiece = targetCollection.FirstOrDefault(p => p.Piece == capturedState);
                        if (existingPiece != null)
                        {
                            existingPiece.Count++;
                        }
                        else
                        {
                            targetCollection.Add(new CapturedPieceInfo { Piece = capturedState, Count = 1 });
                        }
                    }
                    
                    UpdateViewFromGameState();
                    
                    // Switch timer to next player after move
                    if (currentGameHasTimers)
                    {
                        // Mark that first move is done
                        isFirstMove = false;
                        
                        // Switch to next player's timer
                        isWhitePlayerActive = !isWhitePlayerActive;
                        // Timer continues running for the next player (no need to stop/start)
                    }
                    
                    if (result.IsCheck)
                    {
                        MessageBox.Show("Check!");
                    }

                    if (result.IsCheckmate)
                    {
                        MessageBox.Show("Checkmate!");
                        soundService.PlayCheckmateSound();
                        if (currentGameHasTimers)
                        {
                            StopTimer();
                        }
                    }
                    
                    var fen = gameService.GetFen();
                    File.AppendAllText(pathOfFenFile, $"{fen}\n");
                    
                    // Clear highlights after move
                    ClearAvailableMoves();
                }
                else
                {
                    // If invalid move, try to select new piece if clicked on a piece
                    if (currentCell.State != CellUIState.Empty)
                    {
                        previousActiveCell.Active = false;
                        ClearAvailableMoves();
                        SelectPiece(currentCell);
                    }
                    else
                    {
                        var validMoves = gameService.GetValidMoves(fromPos);
                        IncorrectMoveMessage(currentCell, validMoves);
                    }
                }
                
                if (previousActiveCell.Active)
                {
                    previousActiveCell.Active = false;
                    ClearAvailableMoves();
                }
            }
        }, parameter => parameter is CellViewModel && !IsGameLoaded);

        public ObservableCollection<string> PlayersMoves
        {
            get => playerMoves;
            set
            {
                playerMoves = value;
                OnPropertyChanged();
            }

        }

        public IEnumerable<char> Numbers => "87654321";

        public IEnumerable<char> Letters => "ABCDEFGH";

        public string Fen
        {
            get => fen;
            set
            {
                fen = value;
                OnPropertyChanged();
            }
        }

        public string MoveHistory
        {
            get => moveHistory;
            set
            {
                moveHistory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedMoveHistory));
            }
        }

        public string FormattedMoveHistory
        {
            get
            {
                if (string.IsNullOrEmpty(moveHistory))
                    return string.Empty;

                // Format move history with line breaks for better vertical display
                // Split by move numbers (e.g., "1. ", "2. ", etc.)
                var formatted = System.Text.RegularExpressions.Regex.Replace(
                    moveHistory,
                    @"(\d+\.\s)",
                    "\n$1"
                ).TrimStart('\n');

                return formatted;
            }
        }

        public bool ShowFenNotation
        {
            get => showFenNotation;
            set
            {
                showFenNotation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowMoveHistory));
                OnPropertyChanged(nameof(NotationToggleText));
            }
        }

        public bool ShowMoveHistory => !showFenNotation;

        public string NotationToggleText => showFenNotation ? "Show Moves" : "Show FEN";

        public ICommand ToggleNotationCommand => toggleNotationCommand ??= new RelayCommand(parameter =>
        {
            ShowFenNotation = !ShowFenNotation;
        });

        public ICommand NewGameCommand => newGameCommand ??= new RelayCommand(parameter =>
        {
            gameService.StartNewGame();
            playerMoves = new ObservableCollection<string>();
            moves = new ObservableCollection<string>();
            var initialFen = gameService.GetFen();
            File.WriteAllText(pathOfFenFile, $"{DateTime.Now}\n{initialFen}\n");
            
            Fen = initialFen;
            MoveHistory = string.Empty;

            // Clear captured pieces
            CapturedByWhite.Clear();
            CapturedByBlack.Clear();

            // Reset loaded game state
            IsGameLoaded = false;
            loadedGameMoves.Clear();
            SelectedMoveIndex = -1;
            MoveHistoryItems.Clear();
            StopAutoPlay();

            SetupBoard();
            
            // Apply timer settings from checkbox to current game
            currentGameHasTimers = isTimerEnabled && selectedTimeOption != null;
            OnPropertyChanged(nameof(ShowTimers));
            
            // Initialize timers after board setup (but don't start yet - will start when white player selects a piece for first move)
            if (currentGameHasTimers)
            {
                WhitePlayerTime = selectedTimeOption.Time;
                BlackPlayerTime = selectedTimeOption.Time;
                // White player starts, so white timer should be active when they select a piece
                isWhitePlayerActive = true;
                isFirstMove = true; // Reset first move flag
                StopTimer(); // Don't start timer yet - wait for player to select a piece for first move
            }
            else
            {
                StopTimer();
                isFirstMove = false;
            }
            
            // Close game panel after starting new game
            IsGamePanelVisible = false;
        });

        /// <summary>
        /// Selects a piece and shows valid moves
        /// </summary>
        private void SelectPiece(CellViewModel cell)
        {
            // Check if game ended due to time expiration
            if (currentGameHasTimers && !gameTimer.IsEnabled)
            {
                if (whitePlayerTime.TotalSeconds <= 0 || blackPlayerTime.TotalSeconds <= 0)
                {
                    MessageBox.Show("Игра завершена из-за истечения времени. Начните новую игру.", "Игра завершена", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            
            // Clear previous available moves
            ClearAvailableMoves();
            
            var pos = new ChessLib.Position(cell.Position.Horizontal, cell.Position.Vertical);
            var validMoves = gameService.GetValidMoves(pos);
            
            // Start timer when white player selects a piece for the FIRST move only
            if (currentGameHasTimers && isFirstMove && !gameTimer.IsEnabled)
            {
                var boardState = gameService.GetBoardState();
                // Check if selected piece belongs to current player (should be white for first move)
                bool isWhitePiece = cell.State == CellUIState.WhitePawn ||
                                   cell.State == CellUIState.WhiteRook ||
                                   cell.State == CellUIState.WhiteKnight ||
                                   cell.State == CellUIState.WhiteBishop ||
                                   cell.State == CellUIState.WhiteQueen ||
                                   cell.State == CellUIState.WhiteKing;
                
                // Only start timer if it's white's first move and they selected a white piece
                if (isWhitePiece && boardState.CurrentPlayerColor == PieceColor.White)
                {
                    isWhitePlayerActive = true;
                    StartTimer();
                }
            }
            
            // Highlight valid moves on board if enabled in settings
            if (settingsViewModel.ShowAvailableMoves)
            {
                foreach (var move in validMoves)
                {
                    var boardCell = Board.FirstOrDefault(c => c.Position.Horizontal == move.X && c.Position.Vertical == move.Y);
                    if (boardCell != null)
                    {
                        boardCell.AvailableMove = true;
                    }
                }
            }
            
            cell.Active = true;
        }

        /// <summary>
        /// Clears all available move highlights
        /// </summary>
        private void ClearAvailableMoves()
        {
            foreach (var cell in Board)
            {
                cell.AvailableMove = false;
            }
        }

        /// <summary>
        /// Updates the view based on current game state
        /// </summary>
        private void UpdateViewFromGameState()
        {
            var boardState = gameService.GetBoardState();
            
            // Update board representation
            ApplyBoardState(boardState);
            
            // Clear highlights when board state changes
            ClearAvailableMoves();
            
            // Update FEN notation and move history
            Fen = gameService.GetFen();
            MoveHistory = gameService.GetMoveHistory();
            
            // Update move history items - always call this to ensure UI is updated
            UpdateMoveHistoryItems();
        }

        /// <summary>
        /// Applies board state snapshot to the UI board
        /// </summary>
        private void ApplyBoardState(BoardStateSnapshot boardState)
        {
            // Update board cells based on pieces
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var pieceInfo = boardState.Cells[j, i];
                    if (pieceInfo != null)
                    {
                        // Convert piece to CellUiState enum and update board
                        Board[7 - i, j] = GetStateFromPiece(pieceInfo.Piece);
                    }
                    else
                    {
                        Board[7 - i, j] = CellUIState.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Converts IPiece to CellUiState enum
        /// </summary>
        private CellUIState GetStateFromPiece(IPiece piece)
        {
            bool isWhite = piece.Color == PieceColor.White;
            
            return piece switch
            {
                Pawn => isWhite ? CellUIState.WhitePawn : CellUIState.BlackPawn,
                Rook => isWhite ? CellUIState.WhiteRook : CellUIState.BlackRook,
                Knight => isWhite ? CellUIState.WhiteKnight : CellUIState.BlackKnight,
                Bishop => isWhite ? CellUIState.WhiteBishop : CellUIState.BlackBishop,
                Queen => isWhite ? CellUIState.WhiteQueen : CellUIState.BlackQueen,
                King => isWhite ? CellUIState.WhiteKing : CellUIState.BlackKing,
                _ => CellUIState.Empty
            };
        }

        /// <summary>
        /// Collection of pieces captured by white player
        /// </summary>
        public ObservableCollection<CapturedPieceInfo> CapturedByWhite
        {
            get => capturedByWhite;
            set
            {
                capturedByWhite = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Collection of pieces captured by black player
        /// </summary>
        public ObservableCollection<CapturedPieceInfo> CapturedByBlack
        {
            get => capturedByBlack;
            set
            {
                capturedByBlack = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Updates captured pieces lists from move history
        /// </summary>
        private void UpdateCapturedPieces()
        {
            CapturedByWhite.Clear();
            CapturedByBlack.Clear();

            if (gameService?.CurrentGame?.MoveHistory == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdateCapturedPieces: MoveHistory is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"UpdateCapturedPieces: Processing {gameService.CurrentGame.MoveHistory.Count} moves");

            foreach (var move in gameService.CurrentGame.MoveHistory)
            {
                if (move.CapturedPiece != null)
                {
                    var capturedState = GetStateFromPiece(move.CapturedPiece);
                    // Add to the collection of the player who captured it
                    var targetCollection = move.PlayerColor == PieceColor.White ? CapturedByWhite : CapturedByBlack;
                    var existingPiece = targetCollection.FirstOrDefault(p => p.Piece == capturedState);
                    if (existingPiece != null)
                    {
                        existingPiece.Count++;
                    }
                    else
                    {
                        targetCollection.Add(new CapturedPieceInfo { Piece = capturedState, Count = 1 });
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"UpdateCapturedPieces: White captured {CapturedByWhite.Count}, Black captured {CapturedByBlack.Count}");
        }

        private static void IncorrectMoveMessage(CellViewModel CurrentCell, List<ChessLib.Position> ValidMoves)
        {
            string info = "";
            foreach (var move in ValidMoves)
            {
                info += $"\t{"ABCDEFGH"[move.X]}{move.Y + 1}\n";
            }
            MessageBox.Show($"Данная фигура не может пойти на клетку:{CurrentCell.Position}\nДоступные ходы: \n{info}");
        }

        /// <summary>
        /// Sets up initial board positions for WPF
        /// </summary>
        private void SetupBoard()
        {
            // Update view from game state
            UpdateViewFromGameState();
            playerMoves = new ObservableCollection<string>();
            
            // Clear any highlights
            ClearAvailableMoves();
        }

        // Timer properties and methods
        public Models.TimeOption SelectedTimeOption
        {
            get => selectedTimeOption;
            set
            {
                selectedTimeOption = value;
                if (value != null)
                {
                    initialTimePerPlayer = value.Time;
                    OnPropertyChanged(nameof(InitialTimePerPlayer));
                }
                OnPropertyChanged();
            }
        }

        public TimeSpan InitialTimePerPlayer
        {
            get => initialTimePerPlayer;
            set
            {
                initialTimePerPlayer = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan WhitePlayerTime
        {
            get => whitePlayerTime;
            set
            {
                whitePlayerTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WhitePlayerTimeFormatted));
            }
        }

        public TimeSpan BlackPlayerTime
        {
            get => blackPlayerTime;
            set
            {
                blackPlayerTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BlackPlayerTimeFormatted));
            }
        }

        public string WhitePlayerTimeFormatted => FormatTime(whitePlayerTime);
        public string BlackPlayerTimeFormatted => FormatTime(blackPlayerTime);

        public bool IsTimerEnabled
        {
            get => isTimerEnabled;
            set
            {
                isTimerEnabled = value;
                OnPropertyChanged();
                // This setting only applies to next game, not current game
            }
        }

        /// <summary>
        /// Whether timers should be displayed (current game has timers enabled)
        /// </summary>
        public bool ShowTimers
        {
            get => currentGameHasTimers;
        }

        private string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
            return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isWhitePlayerActive)
            {
                if (whitePlayerTime.TotalSeconds > 0)
                {
                    WhitePlayerTime = whitePlayerTime.Subtract(TimeSpan.FromSeconds(1));
                }
                else
                {
                    // Time expired - white loses
                    StopTimer();
                    EndGameByTime(PieceColor.White);
                    MessageBox.Show("Время белых истекло! Черные выиграли по времени.", "Время истекло", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (blackPlayerTime.TotalSeconds > 0)
                {
                    BlackPlayerTime = blackPlayerTime.Subtract(TimeSpan.FromSeconds(1));
                }
                else
                {
                    // Time expired - black loses
                    StopTimer();
                    EndGameByTime(PieceColor.Black);
                    MessageBox.Show("Время черных истекло! Белые выиграли по времени.", "Время истекло", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Ends the game due to time expiration
        /// </summary>
        private void EndGameByTime(PieceColor losingColor)
        {
            // Stop the timer
            StopTimer();
            
            // Mark game as over in the game service
            gameService.EndGameByTime(losingColor);
            
            // Play sound for game over
            soundService.PlayCheckmateSound();
        }

        private void StartTimer()
        {
            if (isTimerEnabled)
            {
                gameTimer.Start();
            }
        }

        private void StopTimer()
        {
            gameTimer.Stop();
        }

    }
}