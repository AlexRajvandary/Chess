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

        /// <summary>
        /// Loads a game from GameRecord
        /// </summary>
        private void LoadGameFromRecord(GameRecord gameRecord)
        {
            try
            {
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
            MoveHistoryItems.Clear();
            
            if (!IsGameLoaded || loadedGameMoves == null || loadedGameMoves.Count == 0)
            {
                // For non-loaded games, use formatted text
                if (!string.IsNullOrEmpty(MoveHistory))
                {
                    MoveHistoryItems.Add(new MoveDisplayItem
                    {
                        MoveNumber = 0,
                        WhiteMoveText = MoveHistory,
                        BlackMoveText = null
                    });
                }
                return;
            }

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
                    
                    if (result.IsCheck)
                    {
                        MessageBox.Show("Check!");
                    }

                    if (result.IsCheckmate)
                    {
                        MessageBox.Show("Checkmate!");
                        soundService.PlayCheckmateSound();
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
            
            // Close game panel after starting new game
            IsGamePanelVisible = false;
        });

        /// <summary>
        /// Selects a piece and shows valid moves
        /// </summary>
        private void SelectPiece(CellViewModel cell)
        {
            // Clear previous available moves
            ClearAvailableMoves();
            
            var pos = new ChessLib.Position(cell.Position.Horizontal, cell.Position.Vertical);
            var validMoves = gameService.GetValidMoves(pos);
            
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
            
            // Update move history items if game is loaded
            if (IsGameLoaded)
            {
                UpdateMoveHistoryItems();
            }
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
    }
}