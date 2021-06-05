using ChessLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ChessBoard
{
    public class MainViewModel : NotifyPropertyChanged
    {
        int currentPlayer;
        private Board _board = new Board();
        private ICommand _newGameCommand;
        
        private ICommand _cellCommand;
        List<Player> players;

        private Game game;
        private List<IPiece> pieces;

        public List<string> playerMoves;

        public IEnumerable<string> PlayersMoves {
            get
            {
               return playerMoves;
            }
        }
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
        /// <summary>
        /// При нажатии на кнопку new game
        /// </summary>
        public ICommand NewGameCommand => _newGameCommand ??= new RelayCommand(parameter =>
        {
            game = new Game();
            SetupBoard();
        });
       
        /// <summary>
        /// Нажатии на клетку выполняется данный метод 
        /// </summary>
        public ICommand CellCommand => _cellCommand ??= new RelayCommand(parameter =>
        {

            Cell CurrentCell = (Cell)parameter;//текущая активная клетка, вернее cell.active у нее false сейчас, но эта именно та клетка на которую мы сейчас нажали

            List<(int, int)> ValidMoves = new List<(int, int)>();//доступные ходы для текущей фигуры

            List<(int, int)> ValidAttacks = new List<(int, int)>();//доступные атаки для текущей фигуры

            Cell PreviousActiveCell = Board.FirstOrDefault(x => x.Active);//предыдущая активная клетка, на которую нажали до текущей (т.е. как бы в первый раз мы выбрали фигуру, а потом, когда нажали на вторую клетку, выбрали куда пойти )

            if (game.gameField.IsCheck())
            {
               
                IsKingWasChosed(CurrentCell, PreviousActiveCell);
                //атака королем под шахом
                ValidAttacks = KingAttackCheck(CurrentCell, ValidAttacks, PreviousActiveCell);
                //ход королем под шахом
                ValidMoves = KingMoveCheck(CurrentCell, ValidMoves, PreviousActiveCell);
            }
            else
            {
                
                ChosePiece(CurrentCell, PreviousActiveCell);
                ValidAttacks = Attack(CurrentCell, ValidAttacks, PreviousActiveCell);
                ValidMoves = Move(CurrentCell, ValidMoves, PreviousActiveCell);
                game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
                if (game.gameField.IsCheck())
                {
                    MessageBox.Show("Шах!");
                }
            }

        }, parameter => parameter is Cell cell && (Board.Any(x => x.Active) || cell.State != State.Empty));

        /// <summary>
        /// Выбран ли король (при шахе)
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="PreviousActiveCell">предыдущая клетка</param>
        private void IsKingWasChosed(Cell CurrentCell, Cell PreviousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PreviousActiveCell is null)
            {

                MessageBox.Show("Шах! Необходимо переставитьь короля!");
                if (game.gameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece is King)
                {

                    CurrentCell.Active = !CurrentCell.Active;

                }
                else
                {
                    MessageBox.Show("Шах! Необходимо переставитьь короля!");

                }
            }
        }
        /// <summary>
        /// Атака королем под шахом
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="ValidAttacks">Доступные клетки для атаки</param>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private List<(int, int)> KingAttackCheck(Cell CurrentCell, List<(int, int)> ValidAttacks, Cell PreviousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PreviousActiveCell != null)
            {
                game.Update(pieces);
                game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
                CurrentCell.Active = true;
                PreviousActiveCell.Active = false;
                King king = (King)game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidAttacks = king.AvailableKills(game.GetGameField(pieces));
                var AvailableAttacksForKingInCheck = ValidAttacks.FindAll(x => game.gameField.GetAtackStatus(pieces, x, GetGameFieldString()));
                foreach (var removedMoves in AvailableAttacksForKingInCheck)
                {
                    ValidAttacks.Remove(removedMoves);
                }
                if (ValidAttacks.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    CurrentCell.State = PreviousActiveCell.State;
                    PreviousActiveCell.Active = false;
                    PreviousActiveCell.State = State.Empty;
                    game.CheckIfPieceWasKilled((PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical), (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), GetGameFieldString(), pieces);
                    game.Update(pieces);
                    currentPlayer++;
                    if (currentPlayer >= 2)
                    {
                        currentPlayer -= 2;
                    }
                    MainWindow.AddNewWhiteMove(CurrentCell.Position.ToString());
                }
                else
                {
                    string info = "";
                    foreach (var i in ValidAttacks)
                    {
                        info += $"/t{"ABCDEFGH"[i.Item1]}{i.Item2 + 1}/n";
                    }
                    MessageBox.Show($"Король не может атаковать клетку {CurrentCell.Position.ToString()}: \n{info}");

                }
            }
            game.Update(pieces);
            game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
            return ValidAttacks;
        }
        /// <summary>
        /// Ход королем под шахом
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="ValidMoves">Доступные клетки для хода</param>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private List<(int, int)> KingMoveCheck(Cell CurrentCell, List<(int, int)> ValidMoves, Cell PreviousActiveCell)
        {
            if (CurrentCell.State == State.Empty && PreviousActiveCell != null)
            {
                game.Update(pieces);
                game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
                CurrentCell.Active = true;
                PreviousActiveCell.Active = false;
                King king = (King)game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidMoves = king.AvailableMoves(game.GetGameField(pieces));
                var AvailableMovesForKingInCheck = ValidMoves.FindAll(x => game.gameField.GetAtackStatus(pieces, x, GetGameFieldString()));
                foreach (var removedMoves in AvailableMovesForKingInCheck)
                {
                    ValidMoves.Remove(removedMoves);
                }
                
                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {
                    PreviousActiveCell.Active = false;
                    CurrentCell.State = PreviousActiveCell.State;
                    PreviousActiveCell.State = State.Empty;
                    game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.Position = (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical);//переставляем фигуру в модели

                    currentPlayer++;
                    if (currentPlayer >= 2)
                    {
                        currentPlayer -= 2;
                    }
                    MainWindow.AddNewWhiteMove(CurrentCell.Position.ToString());

                }
                else
                {
                    string info = "";
                    foreach (var move in ValidMoves)
                    {
                        info += $"\t{"ABCDEFGH"[move.Item1]}{move.Item2 + 1}\n";
                    }
                    MessageBox.Show($"Король не может пойти на клетку: {CurrentCell.Position}\n\t Доступные ходы: \n{info}");

                }
            }
            game.Update(pieces);
            game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
            return ValidMoves;
        }
        /// <summary>
        /// проверет выбрал ли игрок правильную фигуру
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        private void ChosePiece(Cell CurrentCell, Cell PreviousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PreviousActiveCell == null)
            {

                game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);


                if (game.gameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece.Color == players[currentPlayer % 2].Color)
                {
                    CurrentCell.Active = !CurrentCell.Active;

                }
                else
                {
                    MessageBox.Show("Не ваша фигура!");
                }

            }
        }
        /// <summary>
        /// Ход, проверяет можно ли сделать ход и делает его, либо сообщает об ошибке
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="ValidMoves">Доступные клетки для хода</param>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private List<(int, int)> Move(Cell CurrentCell, List<(int, int)> ValidMoves, Cell PreviousActiveCell)
        {
            if (CurrentCell.State == State.Empty && PreviousActiveCell != null)
            {

                game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
                IPiece piece = game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;
                ValidMoves = piece.AvailableMoves(game.GetGameField(pieces));
                List<(int, int)> validMovesWithoutCheckCheck = new List<(int, int)>();
                if (piece is King)
                {
                    var InvalidMovesWithoutCheckCheck = ValidMoves.FindAll(x => game.gameField.GetAtackStatus(pieces, x, GetGameFieldString()));
                    foreach(var move in InvalidMovesWithoutCheckCheck)
                    {
                        ValidMoves.Remove(move);
                    }
                    
                }

                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {
                    PreviousActiveCell.Active = false;
                    CurrentCell.State = PreviousActiveCell.State;
                    PreviousActiveCell.State = State.Empty;
                    game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.Position = (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical);//переставляем фигуру в модели
                    game.Update(pieces);
                    game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
                    currentPlayer++;
                    if (currentPlayer >= 2)
                    {
                        currentPlayer -= 2;
                    }
                    MainWindow.AddNewWhiteMove(CurrentCell.Position.ToString());
                }
                else
                {
                    string info = "";
                    foreach (var move in ValidMoves)
                    {
                        info += $"\t{"ABCDEFGH"[move.Item1]}{move.Item2 + 1}\n";
                    }
                    MessageBox.Show($"Данная фигура не может пойти на клетку:{CurrentCell.Position}\nДоступные ходы: \n{info}");
                }

            }
            
            return ValidMoves;
        }

       

        /// <summary>
        /// Проверяет можно ли атаковать и атакует, либо выводит сообщение об ошибке
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="ValidAttacks">Доступные клетки для атаки</param>>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private List<(int, int)> Attack(Cell CurrentCell, List<(int, int)> ValidAttacks, Cell PreviousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PreviousActiveCell != null)
            {

                game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);

                //если игрок захотел поменять выбранную фигуру
                if (game.gameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece.Color == players[currentPlayer % 2].Color)
                {
                    PreviousActiveCell.Active = !PreviousActiveCell.Active;
                    CurrentCell.Active = true;

                }
                else//если игрок хочет съесть фигуру
                {
                    game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
                    IPiece piece = game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;
                    ValidAttacks = piece.AvailableKills(game.GetGameField(pieces));

                    if (ValidAttacks.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                    {
                        CurrentCell.State = PreviousActiveCell.State;
                        PreviousActiveCell.Active = false;
                        PreviousActiveCell.State = State.Empty;
                        game.CheckIfPieceWasKilled((PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical), (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), GetGameFieldString(), pieces);
                        game.Update(pieces);
                        currentPlayer++;
                        if (currentPlayer >= 2)
                        {
                            currentPlayer -= 2;
                        }
                        if(game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.Color == PieceColor.White)
                        {
                            MainWindow.AddNewWhiteMove(CurrentCell.Position.ToString());
                        }
                        else
                        {

                        }
                       
                    }
                    else
                    {
                        string AttackInfo = "";
                        foreach (var validAttack in ValidAttacks)
                        {
                            AttackInfo += $"{"ABCDEFGH"[validAttack.Item1]}{validAttack.Item2 + 1}\n";
                        }
                        MessageBox.Show($"Съесть нельзя! Доступные клетки для атаки\n:{AttackInfo}");
                    }

                }

            }
            game.Update(pieces);
            game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
            return ValidAttacks;
        }
        /// <summary>
        /// Утсанавливает начальные позиции фигурам при старте игры, для WPF
        /// </summary>
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
            playerMoves = new List<string>();
        }
        MainWindow MainWindow;
        public MainViewModel(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

        }
        private string[,] GetGameFieldString()
        {
            string[,] result = game.GetGameField(pieces);
            return result;
        }
    }
}