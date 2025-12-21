using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ChessLib;
using ChessWPF.Models;
using ChessWPF.Services;

namespace ChessWPF.ViewModels
{
    public class GameViewModel : NotifyPropertyChanged
    {
        private const string pathOfFenFile = "fen.txt";
        private BoardViewModel board;
        private ICommand cellCommand;
        private string fen;
        private ChessGameService gameService;
        private string moveHistory;
        private ObservableCollection<string> moves;
        private ICommand newGameCommand;
        private ObservableCollection<string> playerMoves;
        private readonly SoundService soundService;
        private bool showFenNotation = true;
        private ICommand toggleNotationCommand;

        public GameViewModel(
            ChessGameService gameService,
            SoundService soundService,
            TimerViewModel timerViewModel,
            CapturedPiecesViewModel capturedPiecesViewModel,
            MoveHistoryViewModel moveHistoryViewModel,
            SettingsViewModel settingsViewModel,
            PanelManagementViewModel panelManagementViewModel)
        {
            this.gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            this.soundService = soundService ?? throw new ArgumentNullException(nameof(soundService));
            this.timerViewModel = timerViewModel ?? throw new ArgumentNullException(nameof(timerViewModel));
            this.capturedPiecesViewModel = capturedPiecesViewModel ?? throw new ArgumentNullException(nameof(capturedPiecesViewModel));
            this.moveHistoryViewModel = moveHistoryViewModel ?? throw new ArgumentNullException(nameof(moveHistoryViewModel));
            this.settingsViewModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));
            this.panelManagementViewModel = panelManagementViewModel ?? throw new ArgumentNullException(nameof(panelManagementViewModel));
            Board = new BoardViewModel();
            moves = new ObservableCollection<string>();
            playerMoves = new ObservableCollection<string>();
            SetupBoard();
        }

        private readonly TimerViewModel timerViewModel;
        private readonly CapturedPiecesViewModel capturedPiecesViewModel;
        private readonly MoveHistoryViewModel moveHistoryViewModel;
        private readonly SettingsViewModel settingsViewModel;
        private readonly PanelManagementViewModel panelManagementViewModel;
        public Action OnGameStateUpdated { get; set; }
        
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
                if (currentCell.State == CellUIState.Empty)
                {
                    return;
                }
                SelectPiece(currentCell);
            }
            else if (previousActiveCell == currentCell)
            {
                previousActiveCell.Active = false;
                ClearAvailableMoves();
            }
            else
            {
                if (timerViewModel.IsTimeExpired)
                {
                    MessageBox.Show("Игра завершена из-за истечения времени. Начните новую игру.", "Игра завершена", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var fromPos = new ChessLib.Position(previousActiveCell.Position.Horizontal, previousActiveCell.Position.Vertical);
                var result = gameService.MakeMove(fromPos, currentPos);
                if (result.IsValid)
                {
                    soundService.PlayMoveSound(result);
                    if (result.CapturedPiece != null)
                    {
                        var capturedState = GetStateFromPiece(result.CapturedPiece);
                        bool isWhiteCapturing = previousActiveCell.State == CellUIState.WhitePawn ||
                                               previousActiveCell.State == CellUIState.WhiteRook ||
                                               previousActiveCell.State == CellUIState.WhiteKnight ||
                                               previousActiveCell.State == CellUIState.WhiteBishop ||
                                               previousActiveCell.State == CellUIState.WhiteQueen ||
                                               previousActiveCell.State == CellUIState.WhiteKing;
                        var capturingColor = isWhiteCapturing ? PieceColor.White : PieceColor.Black;
                        capturedPiecesViewModel.AddCapturedPiece(capturingColor, capturedState);
                    }
                    UpdateViewFromGameState();
                    moveHistoryViewModel.UpdateMoveHistoryItems();
                    timerViewModel.SwitchToNextPlayer();
                    if (result.IsCheck)
                    {
                        MessageBox.Show("Check!");
                    }
                    if (result.IsCheckmate)
                    {
                        MessageBox.Show("Checkmate!");
                        soundService.PlayCheckmateSound();
                        timerViewModel.OnGameEnd();
                    }
                    var fen = gameService.GetFen();
                    File.AppendAllText(pathOfFenFile, $"{fen}\n");
                    ClearAvailableMoves();
                }
                else
                {
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
        }, parameter => parameter is CellViewModel && !moveHistoryViewModel.IsGameLoaded);
        
        public string Fen
        {
            get => fen;
            set
            {
                fen = value;
                OnPropertyChanged();
            }
        }
        
        public string FormattedMoveHistory
        {
            get
            {
                if (string.IsNullOrEmpty(moveHistory))
                    return string.Empty;
                var formatted = System.Text.RegularExpressions.Regex.Replace(
                    moveHistory,
                    @"(\d+\.\s)",
                    "\n$1"
                ).TrimStart('\n');
                return formatted;
            }
        }

        public ChessGameService GameService => gameService;

        public IEnumerable<char> Letters => "ABCDEFGH";

        public string MoveHistory
        {
            get => moveHistory;
            set
            {
                moveHistory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedMoveHistory));
                moveHistoryViewModel.UpdateMoveHistoryItems();
            }
        }

        public IEnumerable<char> Numbers => "87654321";

        public string NotationToggleText => showFenNotation ? "Show Moves" : "Show FEN";
        
        public ICommand NewGameCommand => newGameCommand ??= new RelayCommand(parameter =>
        {
            gameService.StartNewGame();
            playerMoves = new ObservableCollection<string>();
            moves = new ObservableCollection<string>();
            var initialFen = gameService.GetFen();
            File.WriteAllText(pathOfFenFile, $"{DateTime.Now}\n{initialFen}\n");
            Fen = initialFen;
            MoveHistory = string.Empty;
            capturedPiecesViewModel.Clear();
            moveHistoryViewModel.ClearLoadedGame();
            SetupBoard();
            OnPropertyChanged(nameof(Board));
            moveHistoryViewModel.UpdateMoveHistoryItems();
            timerViewModel.InitializeForNewGame();
            panelManagementViewModel.IsGamePanelVisible = false;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            OnGameStateUpdated?.Invoke();
        });

        public ObservableCollection<string> PlayersMoves
        {
            get => playerMoves;
            set
            {
                playerMoves = value;
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

        public ICommand ToggleNotationCommand => toggleNotationCommand ??= new RelayCommand(parameter =>
        {
            ShowFenNotation = !ShowFenNotation;
        });

        public void ClearAvailableMoves()
        {
            foreach (var cell in Board)
            {
                cell.AvailableMove = false;
            }
        }

        public CellUIState GetStateFromPiece(IPiece piece)
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

        public void SetupBoard()
        {
            UpdateViewFromGameState();
            playerMoves = new ObservableCollection<string>();
            ClearAvailableMoves();
        }

        public void UpdateViewFromGameState()
        {
            var boardState = gameService.GetBoardState();
            ApplyBoardState(boardState);
            ClearAvailableMoves();
            Fen = gameService.GetFen();
            MoveHistory = gameService.GetMoveHistory();
            moveHistoryViewModel.UpdateMoveHistoryItems();
        }

        private void ApplyBoardState(BoardStateSnapshot boardState)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var pieceInfo = boardState.Cells[j, i];
                    if (pieceInfo != null)
                    {
                        Board[7 - i, j] = GetStateFromPiece(pieceInfo.Piece);
                    }
                    else
                    {
                        Board[7 - i, j] = CellUIState.Empty;
                    }
                }
            }
            OnPropertyChanged(nameof(Board));
        }

        private static void IncorrectMoveMessage(CellViewModel currentCell, List<ChessLib.Position> validMoves)
        {
            string info = "";
            foreach (var move in validMoves)
            {
                info += $"\t{"ABCDEFGH"[move.X]}{move.Y + 1}\n";
            }
            MessageBox.Show($"Данная фигура не может пойти на клетку:{currentCell.Position}\nДоступные ходы: \n{info}");
        }
        private void SelectPiece(CellViewModel cell)
        {
            if (timerViewModel.IsTimeExpired)
            {
                MessageBox.Show("Игра завершена из-за истечения времени. Начните новую игру.", "Игра завершена", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ClearAvailableMoves();
            var pos = new ChessLib.Position(cell.Position.Horizontal, cell.Position.Vertical);
            var validMoves = gameService.GetValidMoves(pos);
            var boardState = gameService.GetBoardState();
            bool isWhitePiece = cell.State == CellUIState.WhitePawn ||
                               cell.State == CellUIState.WhiteRook ||
                               cell.State == CellUIState.WhiteKnight ||
                               cell.State == CellUIState.WhiteBishop ||
                               cell.State == CellUIState.WhiteQueen ||
                               cell.State == CellUIState.WhiteKing;
            timerViewModel.OnFirstPieceSelected(isWhitePiece, boardState.CurrentPlayerColor);
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

    }
}