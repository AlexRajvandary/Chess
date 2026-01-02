using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ChessLib;
using ChessLib.Common;
using ChessLib.Pieces;
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
        private IGameService gameService;
        private string moveHistory;
        private ObservableCollection<string> moves;
        private ICommand newGameCommand;
        private ObservableCollection<string> playerMoves;
        private readonly SoundService soundService;
        private bool showFenNotation = true;
        private ICommand toggleNotationCommand;
        private string blackPlayerName;
        private string eventAndDate;
        private bool isHistoricalGameLoaded = false;
        private string whitePlayerName;
        private readonly TimerViewModel timerViewModel;
        private readonly CapturedPiecesViewModel capturedPiecesViewModel;
        private readonly MoveHistoryViewModel moveHistoryViewModel;
        private readonly SettingsViewModel settingsViewModel;
        private readonly PanelManagementViewModel panelManagementViewModel;

        public GameViewModel(
            IGameService gameService,
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
        }

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

        public ICommand CellCommand => cellCommand ??= new RelayCommand(
            parameter =>
            {
                if (parameter is not CellViewModel currentCell)
                {
                    return;
                }

                HandleCellClick(currentCell);
            },
            parameter => parameter is CellViewModel && !moveHistoryViewModel.IsGameLoaded);

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

        public IGameService GameService => gameService;

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
            IsHistoricalGameLoaded = false;
            EventAndDate = null;
            WhitePlayerName = null;
            BlackPlayerName = null;
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
        public string BlackPlayerName
        {
            get => blackPlayerName;
            set
            {
                blackPlayerName = value;
                OnPropertyChanged();
            }
        }
        public string EventAndDate
        {
            get => eventAndDate;
            set
            {
                eventAndDate = value;
                OnPropertyChanged();
            }
        }
        public bool IsHistoricalGameLoaded
        {
            get => isHistoricalGameLoaded;
            set
            {
                isHistoricalGameLoaded = value;
                OnPropertyChanged();
            }
        }
        public string WhitePlayerName
        {
            get => whitePlayerName;
            set
            {
                whitePlayerName = value;
                OnPropertyChanged();
            }
        }
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

        private CellUIState GetStateFromPieceInfo(IPieceInfo pieceInfo)
        {
            if (pieceInfo == null)
                return CellUIState.Empty;

            bool isWhite = pieceInfo.Color == PieceColor.White;
            return pieceInfo.Type switch
            {
                ChessLib.Common.PieceType.Pawn => isWhite ? CellUIState.WhitePawn : CellUIState.BlackPawn,
                ChessLib.Common.PieceType.Rook => isWhite ? CellUIState.WhiteRook : CellUIState.BlackRook,
                ChessLib.Common.PieceType.Knight => isWhite ? CellUIState.WhiteKnight : CellUIState.BlackKnight,
                ChessLib.Common.PieceType.Bishop => isWhite ? CellUIState.WhiteBishop : CellUIState.BlackBishop,
                ChessLib.Common.PieceType.Queen => isWhite ? CellUIState.WhiteQueen : CellUIState.BlackQueen,
                ChessLib.Common.PieceType.King => isWhite ? CellUIState.WhiteKing : CellUIState.BlackKing,
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
            var gameState = gameService.GetState();
            ApplyBoardState(gameState);
            ClearAvailableMoves();
            Fen = gameService.GetFen();
            MoveHistory = gameService.GetMoveHistory();
            moveHistoryViewModel.UpdateMoveHistoryItems();
        }

        private void ApplyBoardState(IGameState gameState)
        {
            // Initialize board as empty
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Board[7 - i, j] = CellUIState.Empty;
                }
            }

            // Fill board with pieces from game state
            foreach (var pieceInfo in gameState.Pieces)
            {
                if (pieceInfo != null)
                {
                    var x = pieceInfo.Position.X;
                    var y = pieceInfo.Position.Y;
                    // Convert from library coordinates (0-7) to board view coordinates
                    Board[7 - y, x] = GetStateFromPieceInfo(pieceInfo);
                }
            }
            OnPropertyChanged(nameof(Board));
        }

        public void SetHistoricalGameInfo(HistoricalGame historicalGame)
        {
            if (historicalGame == null)
            {
                IsHistoricalGameLoaded = false;
                EventAndDate = null;
                WhitePlayerName = null;
                BlackPlayerName = null;
                return;
            }
            IsHistoricalGameLoaded = true;
            WhitePlayerName = historicalGame.WhitePlayer ?? "White";
            BlackPlayerName = historicalGame.BlackPlayer ?? "Black";
            string eventText = string.IsNullOrWhiteSpace(historicalGame.Event) ? null : historicalGame.Event;
            string dateText = FormatHistoricalDate(historicalGame.PlayedAt);
            if (!string.IsNullOrWhiteSpace(eventText) && !string.IsNullOrWhiteSpace(dateText))
            {
                EventAndDate = $"{eventText} • {dateText}";
            }
            else if (!string.IsNullOrWhiteSpace(eventText))
            {
                EventAndDate = eventText;
            }
            else if (!string.IsNullOrWhiteSpace(dateText))
            {
                EventAndDate = dateText;
            }
            else
            {
                EventAndDate = null;
            }
        }

        private void HandleCellClick(CellViewModel currentCell)
        {
            var previousActiveCell = Board.FirstOrDefault(x => x.Active);
            var currentPos = new ChessLib.Common.Position(currentCell.Position.Horizontal, currentCell.Position.Vertical);

            if (previousActiveCell is null)
            {
                HandleNoActiveCellClick(currentCell);
                return;
            }

            if (ReferenceEquals(previousActiveCell, currentCell))
            {
                DeselectCurrent(previousActiveCell);
                return;
            }

            HandleMoveOrReselect(previousActiveCell, currentCell, currentPos);
        }

        private void HandleNoActiveCellClick(CellViewModel currentCell)
        {
            if (currentCell.State == CellUIState.Empty)
            {
                return;
            }

            SelectPiece(currentCell);
        }

        private void DeselectCurrent(CellViewModel previousActiveCell)
        {
            previousActiveCell.Active = false;
            ClearAvailableMoves();
        }

        private void HandleMoveOrReselect(
            CellViewModel previousActiveCell,
            CellViewModel currentCell,
            ChessLib.Common.Position currentPos)
        {
            if (timerViewModel.IsTimeExpired)
            {
                MessageBox.Show(
                    "Game ended due to time expiration. Start a new game.",
                    "Game Ended",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var fromPos = new ChessLib.Common.Position(previousActiveCell.Position.Horizontal, previousActiveCell.Position.Vertical);
            var result = gameService.MakeMove(fromPos, currentPos);

            if (result.IsValid)
            {
                HandleValidMove(previousActiveCell, result);
                return;
            }

            HandleInvalidMove(previousActiveCell, currentCell, fromPos);
        }

        private void HandleValidMove(CellViewModel previousActiveCell, MoveResult result)
        {
            soundService.PlayMoveSound(result);

            if (result.CapturedPiece is not null)
            {
                AddCapturedPiece(previousActiveCell, result.CapturedPiece);
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

            // Консистентно завершаем ход
            CleanupAfterAction();
        }

        private void HandleInvalidMove(CellViewModel previousActiveCell, CellViewModel currentCell, ChessLib.Common.Position fromPos)
        {
            if (currentCell.State != CellUIState.Empty)
            {
                // Перевыбор фигуры
                previousActiveCell.Active = false;
                ClearAvailableMoves();
                SelectPiece(currentCell);
                return;
            }

            var validMoves = gameService.GetValidMoves(fromPos);
            IncorrectMoveMessage(currentCell, validMoves);

            CleanupAfterAction();
        }

        private void AddCapturedPiece(CellViewModel previousActiveCell, IPiece capturedPiece)
        {
            var capturedState = GetStateFromPiece(capturedPiece);

            var capturingColor = IsWhiteCellState(previousActiveCell.State)
                ? PieceColor.White
                : PieceColor.Black;

            capturedPiecesViewModel.AddCapturedPiece(capturingColor, capturedState);
        }

        private static bool IsWhiteCellState(CellUIState state)
        {
            return state == CellUIState.WhitePawn
                || state == CellUIState.WhiteRook
                || state == CellUIState.WhiteKnight
                || state == CellUIState.WhiteBishop
                || state == CellUIState.WhiteQueen
                || state == CellUIState.WhiteKing;
        }

        private void CleanupAfterAction()
        {
            ClearAvailableMoves();

            var active = Board.FirstOrDefault(x => x.Active);
            if (active is not null)
            {
                active.Active = false;
            }
        }

        private static string FormatHistoricalDate(DateTime? playedAt)
        {
            if (!playedAt.HasValue)
                return null;
            var date = playedAt.Value;
            if (date.Month == 1 && date.Day == 1)
            {
                return date.Year.ToString();
            }
            return date.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("en-US"));
        }

        private static void IncorrectMoveMessage(CellViewModel currentCell, IReadOnlyList<ChessLib.Common.Position> validMoves)
        {
            string info = "";
            foreach (var move in validMoves)
            {
                info += $"\t{"ABCDEFGH"[move.X]}{move.Y + 1}\n";
            }
            MessageBox.Show($"This piece cannot move to cell: {currentCell.Position}\nAvailable moves: \n{info}");
        }

        private void SelectPiece(CellViewModel cell)
        {
            if (timerViewModel.IsTimeExpired)
            {
                MessageBox.Show("Game ended due to time expiration. Start a new game.", "Game Ended", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ClearAvailableMoves();
            var pos = new ChessLib.Common.Position(cell.Position.Horizontal, cell.Position.Vertical);
            var validMoves = gameService.GetValidMoves(pos);
            var gameState = gameService.GetState();
            bool isWhitePiece = cell.State == CellUIState.WhitePawn ||
                               cell.State == CellUIState.WhiteRook ||
                               cell.State == CellUIState.WhiteKnight ||
                               cell.State == CellUIState.WhiteBishop ||
                               cell.State == CellUIState.WhiteQueen ||
                               cell.State == CellUIState.WhiteKing;

            timerViewModel.OnFirstPieceSelected(isWhitePiece, gameState.CurrentPlayerColor);
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