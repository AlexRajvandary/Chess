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

        public IEnumerable<string> PlayersMoves
        {
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

                MessageBox.Show("Шах! Необходимо переставить короля!");

                if (game.gameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece is King)
                {

                    CurrentCell.Active = !CurrentCell.Active;

                }
                else
                {
                    MessageBox.Show("Выбирите короля!");

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
                //Проверяем атакована ли клетка, на которую собирается пойти король
                var AvailableAttacksForKingInCheck = ValidAttacks.FindAll(x => game.gameField.GetAtackStatus(pieces.Where(x => x.Color != king.Color).ToList(), x, GetGameFieldString()));
                foreach (var removedMoves in AvailableAttacksForKingInCheck)
                {
                    ValidAttacks.Remove(removedMoves);
                }
                if (ValidAttacks.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    king.IsMoved = true;

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
                var EnemyPieces = pieces.Where(x => x.Color != king.Color).ToList();
                var AvailableMovesForKingInCheck = ValidMoves.FindAll(x => game.gameField.GetAtackStatus(EnemyPieces, x, GetGameFieldString()));
                foreach (var removedMoves in AvailableMovesForKingInCheck)
                {
                    ValidMoves.Remove(removedMoves);
                }

                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    king.IsMoved = true;


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
                //Фигура, которую мы выбрали
                IPiece piece = game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;
                //Доступные ходы у фигуры
                ValidMoves = piece.AvailableMoves(game.GetGameField(pieces));
                List<(int, int)> validMovesWithoutCheckCheck = new List<(int, int)>();
                //Доступна ли короткая рокировка
                bool ShortCastle = false;
                //Доступна ли длинная рокировка
                bool LongCastle = false;
                //Вражеские фигуры
                var EnemyPieces = pieces.Where(x => x.Color != piece.Color).ToList();
                //Если выбранная фигура Король, то из доступных ходов нужно убрать, те которые атакованы вражескими фигурами, чтобы не было шаха
                if (piece is King)
                {
                    var InvalidMovesWithoutCheckCheck = ValidMoves.FindAll(x => game.gameField.GetAtackStatus(EnemyPieces, x, GetGameFieldString()));
                    foreach (var move in InvalidMovesWithoutCheckCheck)
                    {
                        ValidMoves.Remove(move);
                    }
                    //Проверяем доступность рокировок
                    var RoyalRook = pieces.Where(somePiece => somePiece.Color == piece.Color).Where(myPiece => myPiece is Rook).Where(SomeRook => ((Rook)SomeRook).RookKind == RookKind.Royal).ToList();
                    var QueenRook = pieces.Where(somePiece => somePiece.Color == piece.Color).Where(myPiece => myPiece is Rook).Where(SomeRook => ((Rook)SomeRook).RookKind == RookKind.Queen).ToList();
                    ShortCastle = ((King)piece).ShortCastling((Rook)RoyalRook[0], game.gameField, EnemyPieces, GetGameFieldString());
                    //Если рокировки доступны, то добавляем их в список доступных ходов
                    if (ShortCastle)
                    {
                        ValidMoves.Add((6, piece.Position.Item2));
                    }
                    LongCastle = ((King)piece).LongCastling((Rook)QueenRook[0], game.gameField, EnemyPieces, GetGameFieldString());
                    if (LongCastle)
                    {
                        ValidMoves.Add((1, piece.Position.Item2));
                    }

                }

                if (piece is Pawn)
                {
                    var Enemypawn = EnemyPieces.Where(x => x.Position.Item2 == piece.Position.Item2).Where(x => x is Pawn).ToList();
                    if (Enemypawn != null)
                    {
                        foreach (var pawn in Enemypawn)
                        {
                            var validPawnMoves = ((Pawn)piece).AvailableKills(game.GetGameField(pieces), (Pawn)pawn);
                            ValidMoves = ValidMoves.Union(validPawnMoves)?.ToList();
                        }


                    }

                }
                //Если ход, который мы собираемся сделать доступен, то делаем
                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {
                    if (ShortCastle && piece is King && (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (6, piece.Position.Item2))
                    {
                        //короткая рокировка
                        ((King)piece).Position = (6, ((King)piece).Position.Item2);
                        var rook = pieces.Where(somePiece => somePiece.Color == piece.Color).Where(myPiece => myPiece is Rook).Where(SomeRook => ((Rook)SomeRook).RookKind == RookKind.Royal).ToList();
                        rook[0].Position = (5, rook[0].Position.Item2);

                        //Отрисовка короткой рокировки
                        /*
                        При обращении к Board через индексы важно помнить, что позиция по горизонтали у Board - вторая координата, позиция по вертикали - первая.
                        Порядок по вертикали сверху вниз, то есть белый король находится на 7 строке у Board
                        */
                        PreviousActiveCell.Active = false;
                        Board[7 - piece.Position.Item2, 6] = PreviousActiveCell.State;
                        PreviousActiveCell.State = State.Empty;
                        Board[7 - piece.Position.Item2, 5] = Board[7 - piece.Position.Item2, 7];
                        Board[7 - piece.Position.Item2, 7] = State.Empty;

                    }
                    else if (LongCastle && piece is King && (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (1, piece.Position.Item2))
                    {
                        //Длинная рокировка
                        ((King)piece).Position = (2, ((King)piece).Position.Item2);
                        var rook = pieces.Where(somePiece => somePiece.Color == piece.Color).Where(myPiece => myPiece is Rook).Where(SomeRook => ((Rook)SomeRook).RookKind == RookKind.Queen).ToList();
                        rook[0].Position = (3, rook[0].Position.Item2);

                        //Отрисовка короткой рокировки
                        /*
                        При обращении к Board через индексы важно помнить, что позиция по горизонтали у Board - вторая координата, позиция по вертикали - первая.
                        Порядок по вертикали сверху вниз, то есть белый король находится на 7 строке у Board
                        */
                        PreviousActiveCell.Active = false;
                        Board[7 - piece.Position.Item2, 2] = PreviousActiveCell.State;
                        PreviousActiveCell.State = State.Empty;
                        Board[7 - piece.Position.Item2, 3] = Board[7 - piece.Position.Item2, 0];
                        Board[7 - piece.Position.Item2, 0] = State.Empty;

                        //Добавляем сделанный ход на listview в главном окне
                        MainWindow.AddNewWhiteMove($"Длинная рокировка {piece.Color}");
                    }
                    else if (EnemyPieces.Where(x => x.Position.Item2 == piece.Position.Item2).Where(x => x is Pawn).ToList().Count > 0 && piece is Pawn)
                    {
                        /*
                         Взятие на проходе
                        En passent
                         */
                        PreviousActiveCell.Active = false;
                        CurrentCell.State = PreviousActiveCell.State;
                        PreviousActiveCell.State = State.Empty;

                        Board[7 - piece.Position.Item2, CurrentCell.Position.Horizontal] = State.Empty;

                        game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.Position = (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical);//переставляем фигуру в модели


                    }
                    else
                    {
                        if (piece is Pawn && (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (((Pawn)piece).StartPos.Item1 + ((Pawn)piece).MoveDir[1].Item1, ((Pawn)piece).StartPos.Item2 + ((Pawn)piece).MoveDir[1].Item2))
                        {
                            ((Pawn)piece).EnPassantAvailable = true;
                        }
                        //Добавляем сделанный ход на listview в главном окне
                        MainWindow.AddNewWhiteMove(CurrentCell.Position.ToString());

                        PreviousActiveCell.Active = false;
                        CurrentCell.State = PreviousActiveCell.State;
                        PreviousActiveCell.State = State.Empty;
                        game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.Position = (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical);//переставляем фигуру в модели

                    }
                    if (piece is King king)
                    {
                        king.IsMoved = true;
                    }
                    if (piece is Rook Rook)
                    {
                        Rook.IsMoved = true;
                    }

                    var enemyPieces = pieces.Where(p => p.Color != piece.Color && p is Pawn);

                    foreach(var EnemyPawn in enemyPieces)
                    {
                        ((Pawn)EnemyPawn).EnPassantAvailable = false;
                    }

                    game.Update(pieces);
                    game.gameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
                    currentPlayer++;
                    if (currentPlayer >= 2)
                    {
                        currentPlayer -= 2;
                    }
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
                    if (piece is King)
                    {
                        var InvalidMovesWithoutCheckCheck = ValidAttacks.FindAll(x => game.gameField.GetAtackStatus(pieces.Where(x => x.Color != piece.Color).ToList(), x, GetGameFieldString()));
                        foreach (var move in InvalidMovesWithoutCheckCheck)
                        {
                            ValidAttacks.Remove(move);
                        }
                    }

                    var InvalidAttacks = ValidAttacks.FindAll(x => game.gameField[x.Item1, x.Item2].Piece is King);
                    foreach (var move in InvalidAttacks)
                    {
                        ValidAttacks.Remove(move);
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
                        string AttackInfo = "";
                        foreach (var validAttack in ValidAttacks)
                        {
                            AttackInfo += $"{"ABCDEFGH"[validAttack.Item1]}{validAttack.Item2 + 1}\n";
                        }
                        MessageBox.Show($"Съесть нельзя! Доступные клетки для атаки\n:{AttackInfo}");
                    }
                    if (piece is King king)
                    {
                        king.IsMoved = true;
                    }
                    if (piece is Rook rook)
                    {
                        rook.IsMoved = true;
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
        public MainWindow MainWindow;
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