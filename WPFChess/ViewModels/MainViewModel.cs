using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ChessLib;

namespace ChessBoard
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private const string pathOfFenFile = "fen.txt";

        private Board board;
        private ICommand cellCommand;
        private int currentPlayer;
        
        private Game game;
        private ObservableCollection<string> moves;
        private ICommand newGameCommand;
        private List<IPiece> pieces;
        private ObservableCollection<string> playerMoves;
        private List<Player> players;

        public MainViewModel()
        {
            Board = new Board();
        }

        public Board Board
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
            Cell currentCell = (Cell)parameter;
            Cell previousActiveCell = Board.FirstOrDefault(x => x.Active);

            // Convert UI positions to chess positions
            var currentPos = new ChessLib.Position(currentCell.Position.Horizontal, currentCell.Position.Vertical);
            
            if (previousActiveCell == null)
            {
                // First click - select piece
                SelectPiece(currentCell);
            }
            else
            {
                // Second click - make move
                var fromPos = new ChessLib.Position(previousActiveCell.Position.Horizontal, previousActiveCell.Position.Vertical);
                
                // Use Game API to make move
                var result = game.MakeMove(fromPos, currentPos);
                
                if (result.IsValid)
                {
                    // Update UI based on game state
                    UpdateViewFromGameState();
                    
                    if (result.IsCheck)
                        MessageBox.Show("Check!");
                    if (result.IsCheckmate)
                        MessageBox.Show("Checkmate!");
                    
                    File.AppendAllText(pathOfFenFile, $"{Fen.GetFenFromTheGameField()}\n");
                }
                else
                {
                    // Show error message
                    var validMoves = game.GetValidMoves(fromPos);
                    IncorrectMoveMessage(currentCell, validMoves);
                }
                
                // Clear selection
                previousActiveCell.Active = false;
            }
        }, parameter => parameter is Cell cell && (Board.Any(x => x.Active) || cell.State != State.Empty));

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

        public ICommand NewGameCommand => newGameCommand ??= new RelayCommand(parameter =>
        {
            game = new Game();
            playerMoves = new ObservableCollection<string>();
            moves = new ObservableCollection<string>();
            File.WriteAllText(pathOfFenFile, $"{DateTime.Now.ToString()}\n");
            Fen.GameField = game.GameField;

            SetupBoard();
        });

        /// <summary>
        /// Selects a piece and shows valid moves
        /// </summary>
        private void SelectPiece(Cell cell)
        {
            var pos = new ChessLib.Position(cell.Position.Horizontal, cell.Position.Vertical);
            var validMoves = game.GetValidMoves(pos);
            
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
            var state = game.GetState();
            pieces = state.Pieces;
            currentPlayer = game.CurrentPlayer;
            
            // Update board representation
            UpdateBoardFromState(state);
        }

        /// <summary>
        /// Updates board UI from game state
        /// </summary>
        private void UpdateBoardFromState(GameState state)
        {
            // Update board cells based on pieces
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var piece = state.Pieces.FirstOrDefault(p => p.Position.X == j && p.Position.Y == i && !p.IsDead);
                    if (piece != null)
                    {
                        // Convert piece to State enum and update board
                        Board[7 - i, j] = GetStateFromPiece(piece);
                    }
                    else
                    {
                        Board[7 - i, j] = State.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Converts IPiece to State enum
        /// </summary>
        private State GetStateFromPiece(IPiece piece)
        {
            bool isWhite = piece.Color == PieceColor.White;
            
            return piece switch
            {
                Pawn => isWhite ? State.WhitePawn : State.BlackPawn,
                Rook => isWhite ? State.WhiteRook : State.BlackRook,
                Knight => isWhite ? State.WhiteKnight : State.BlackKnight,
                Bishop => isWhite ? State.WhiteBishop : State.BlackBishop,
                Queen => isWhite ? State.WhiteQueen : State.BlackQueen,
                King => isWhite ? State.WhiteKing : State.BlackKing,
                _ => State.Empty
            };
        }

        private static void IncorrectMoveMessage(Cell CurrentCell, List<ChessLib.Position> ValidMoves)
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