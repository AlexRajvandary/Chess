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
using ChessWPF.Commands;
using ChessWPF.Windows;

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
        private Brush lightSquareColor;
        private Brush darkSquareColor;
        private readonly SoundService soundService;
        private SettingsViewModel settingsViewModel;
        private bool isSettingsPanelVisible;
        private System.Windows.HorizontalAlignment settingsPanelAlignment = System.Windows.HorizontalAlignment.Left;

        public MainViewModel()
        {
            Board = new BoardViewModel();
            gameService = new ChessGameService();
            // Initialize with default colors
            LightSquareColor = new SolidColorBrush(Color.FromRgb(240, 217, 181)); // Bisque
            DarkSquareColor = new SolidColorBrush(Color.FromRgb(181, 136, 99));   // SandyBrown
            
            // Initialize sound service
            soundService = new SoundService();
            
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
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        });

        public ICommand OpenSettingsCommand => openSettingsCommand ??= new RelayCommand(parameter =>
        {
            IsSettingsPanelVisible = true;
        });

        public ICommand CloseSettingsCommand => closeSettingsCommand ??= new RelayCommand(parameter =>
        {
            IsSettingsPanelVisible = false;
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
                SelectPiece(currentCell);
            }
            else
            {
                var fromPos = new ChessLib.Position(previousActiveCell.Position.Horizontal, previousActiveCell.Position.Vertical);
                var result = gameService.MakeMove(fromPos, currentPos);
                
                if (result.IsValid)
                {
                    soundService.PlayMoveSound(result);
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
                }
                else
                {
                    var validMoves = gameService.GetValidMoves(fromPos);
                    IncorrectMoveMessage(currentCell, validMoves);
                }
                
                previousActiveCell.Active = false;
            }
        }, parameter => parameter is CellViewModel cell && (Board.Any(x => x.Active) || cell.State != CellUIState.Empty));

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

            SetupBoard();
        });

        /// <summary>
        /// Selects a piece and shows valid moves
        /// </summary>
        private void SelectPiece(CellViewModel cell)
        {
            var pos = new ChessLib.Position(cell.Position.Horizontal, cell.Position.Vertical);
            var validMoves = gameService.GetValidMoves(pos);
            
            // Highlight valid moves on board
            foreach (var move in validMoves)
            {
                var boardCell = Board.FirstOrDefault(c => c.Position.Horizontal == move.X && c.Position.Vertical == move.Y);
                if (boardCell != null)
                {
                    // Mark cell as available for move (you may need to add a property for this)
                }
            }
            
            cell.Active = true;
        }

        /// <summary>
        /// Updates the view based on current game state
        /// </summary>
        private void UpdateViewFromGameState()
        {
            var boardState = gameService.GetBoardState();
            
            // Update board representation
            ApplyBoardState(boardState);
            
            // Update FEN notation and move history
            Fen = gameService.GetFen();
            MoveHistory = gameService.GetMoveHistory();
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
        }
    }
}