using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ChessLib;

namespace ChessBoard
{
    public class MainViewModel : NotifyPropertyChanged
    {
     
        int currentPlayer;
        private Board _board = new Board();
        private ICommand _newGameCommand;
        private ICommand _clearCommand;
        private ICommand _cellCommand;
        List<Player> players;

        private static Game game = new Game();
        private static List<IPiece> pieces;
        
        public IEnumerable<char> Numbers => "87654321";
        public IEnumerable<char> Letters => "ABCDEFGH";

        public Board Board 
        {
            get => _board;
            set
            {
                _board = value;
                OnPropertyChanged();
            }
        }

        public ICommand NewGameCommand => _newGameCommand ??= new RelayCommand(parameter => 
        {
            game = new Game();
            SetupBoard();
        });

        public ICommand ClearCommand => _clearCommand ??= new RelayCommand(parameter =>
        {
            game = new Game();
           
            SetupBoard();
        });

        public ICommand CellCommand => _cellCommand ??= new RelayCommand(parameter =>
        {
            Cell cell = (Cell)parameter;//текущая активная клетка, вернее cell.active у нее false сейчас, но эта именно та клетка на которую мы сейчас нажали
           
            List<(int, int)> ValidMoves = new List<(int, int)>();//доступные ходы для текущей фигуры
           
            Cell activeCell = Board.FirstOrDefault(x => x.Active);//предыдущая активная клетка, на которую нажали до текущей (т.е. как бы в первый раз мы выбрали фигуру, а потом, когда нажали на вторую клетку, выбрали куда пойти )
            if (cell.State != State.Empty)
            {
                if (!cell.Active && activeCell != null)
                    activeCell.Active = false;
                
                
                game.gameField.Update(pieces, getGameFieldString(), players[currentPlayer % 2].Color);
                if (game.gameField[cell.Position.Horizontal,cell.Position.Vertical].Piece.Color == players[currentPlayer % 2].Color)
                {
                    cell.Active = !cell.Active;
                   

                }
                else
                {
                    MessageBox.Show("Не ваша фигура!");
                }
                
            }
            else if (activeCell != null)
            {
                game.gameField.Update(pieces, getGameFieldString(), players[currentPlayer % 2].Color);
                IPiece piece = game.gameField[activeCell.Position.Horizontal, activeCell.Position.Vertical].Piece;
                ValidMoves = piece.AvailableMoves(game.GetGameField(pieces));
                
               
                if (ValidMoves.Contains((cell.Position.Horizontal, cell.Position.Vertical)))
                {
                    activeCell.Active = false;
                    cell.State = activeCell.State;
                    activeCell.State = State.Empty;
                    game.gameField[activeCell.Position.Horizontal, activeCell.Position.Vertical].Piece.Position = (cell.Position.Horizontal, cell.Position.Vertical);//переставляем фигуру в модели
                   
                    currentPlayer++;
                }
                else
                {
                    string info = "";
                    foreach(var move in ValidMoves)
                    {
                        info += $"{"ABCDEFG"[move.Item1]}{move.Item2+1}\n";
                    }
                    MessageBox.Show($"Что-то не так,текущий ход:{"ABCDEFGH"[cell.Position.Horizontal]}{cell.Position.Vertical+1} доступные ходы: \n{info}");
                }
             
                
               
                
            }
        }, parameter => parameter is Cell cell && (Board.Any(x => x.Active) || cell.State != State.Empty));

        private void SetupBoard()
        {
            currentPlayer = 0;
            pieces = game.GetPieces();
           players = new List<Player>()
        {
            new Player(ChessLib.PieceColor.White,pieces.Where(x=> x.Color == ChessLib.PieceColor.White).ToList(),"user1"),
            new Player(ChessLib.PieceColor.Black,pieces.Where(x=> x.Color == ChessLib.PieceColor.Black).ToList(),"user2")
        };
            Board board = new Board();
            board[0, 0] = State.BlackRook;
            board[0, 1] = State.BlackKnight;
            board[0, 2] = State.BlackBishop;
            board[0, 3] = State.BlackQueen;
            board[0, 4] = State.BlackKing;
            board[0, 5] = State.BlackBishop;
            board[0, 6] = State.BlackKnight;
            board[0, 7] = State.BlackRook;
            for (int i = 0; i < 8; i++)
            {
                board[1, i] = State.BlackPawn;
                board[6, i] = State.WhitePawn;
            }
            board[7, 0] = State.WhiteRook;
            board[7, 1] = State.WhiteKnight;
            board[7, 2] = State.WhiteBishop;
            board[7, 3] = State.WhiteQueen;
            board[7, 4] = State.WhiteKing;
            board[7, 5] = State.WhiteBishop;
            board[7, 6] = State.WhiteKnight;
            board[7, 7] = State.WhiteRook;
            Board = board;
        }

        public MainViewModel()
        {
           
            
        }
        private string[,] getGameFieldString()
        {
            string[,] result = game.GetGameField(pieces);
            return result;
        }

        private bool IsMyColor(Player player, IPiece piece)
        {
            return player.Color == piece.Color;
        }
    }
}