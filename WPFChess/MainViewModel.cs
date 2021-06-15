using ChessLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ChessBoard
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private const int LongCastleVerticalPosition = 2;

        private const int ShortCastleVerticalPosition = 6;

        int CurrentPlayer;

        private Board _board = new Board();

        private ICommand _newGameCommand;

        private ICommand _cellCommand;

        List<Player> Players;

        List<string> Colors = new List<string>() { "Белые", "Черные" };

        private Game game;

        private List<IPiece> Pieces;

        public ObservableCollection<string> Moves;

        public ObservableCollection<string> playerMoves;

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
            playerMoves = new ObservableCollection<string>();
            Moves = new ObservableCollection<string>();
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
                game.RemoveDeadPieces(Pieces);
                game.gameField.Update(Pieces, GetGameFieldString(), Players[CurrentPlayer % 2].Color);

                IsKingWasChosed(CurrentCell, PreviousActiveCell);
                //атака королем под шахом
                ValidAttacks = KingAttackCheck(CurrentCell, ValidAttacks, PreviousActiveCell);
                //ход королем под шахом
                ValidMoves = KingMoveCheck(CurrentCell, ValidMoves, PreviousActiveCell);
                game.RemoveDeadPieces(Pieces);
                game.gameField.Update(Pieces, GetGameFieldString(), Players[CurrentPlayer % 2].Color);
            }
            else
            {
                game.RemoveDeadPieces(Pieces);
                game.gameField.Update(Pieces, GetGameFieldString(), Players[CurrentPlayer % 2].Color);

                ChosePiece(CurrentCell, PreviousActiveCell);


                //Игрок хочет атаковать
                ValidAttacks = Attack(CurrentCell, ValidAttacks, PreviousActiveCell);

                //Игрок хочет сделать ход
                ValidMoves = Move(CurrentCell, ValidMoves, PreviousActiveCell);

                game.RemoveDeadPieces(Pieces);
                game.gameField.Update(Pieces, GetGameFieldString(), Players[CurrentPlayer % 2].Color);

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

                CurrentCell.Active = true;
                PreviousActiveCell.Active = false;
                King king = (King)game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidAttacks = king.AvailableKills(game.GetGameField(Pieces));

                //Проверяем атакована ли клетка, на которую собирается пойти король
                var AvailableAttacksForKingInCheck = ValidAttacks.FindAll(x => game.gameField.GetAtackStatus(Pieces.Where(x => x.Color != king.Color).ToList(), x, GetGameFieldString()));
                foreach (var removedMoves in AvailableAttacksForKingInCheck)
                {
                    ValidAttacks.Remove(removedMoves);
                }
                if (ValidAttacks.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    king.IsMoved = true;

                    AttackView(CurrentCell, PreviousActiveCell);

                    AttackModel(CurrentCell, PreviousActiveCell);

                    ChangePlayer();

                    ChangeEnPassentStatusForEnimiesPawns(king);

                    
                    //MainWindow.AddNewMove(CurrentCell.Position.ToString());
                    Moves.Add($"{Players[CurrentPlayer].Color} {CurrentCell.Position}");
                    PlayersMoves = Moves;

                }
                else
                {
                    IncorrectKingAttackMessage(CurrentCell, ValidAttacks);
                }
            }

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

                CurrentCell.Active = true;
                PreviousActiveCell.Active = false;
                King king = (King)game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidMoves = king.AvailableMoves(game.GetGameField(Pieces));
                var EnemyPieces = Pieces.Where(x => x.Color != king.Color).ToList();
                var AvailableMovesForKingInCheck = ValidMoves.FindAll(x => game.gameField.GetAtackStatus(EnemyPieces, x, GetGameFieldString()));
                foreach (var removedMoves in AvailableMovesForKingInCheck)
                {
                    ValidMoves.Remove(removedMoves);
                }

                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    king.IsMoved = true;

                    MoveView(CurrentCell, PreviousActiveCell);

                    MoveModel(CurrentCell, PreviousActiveCell);
                    CurrentPlayer++;
                    if (CurrentPlayer >= 2)
                    {
                        CurrentPlayer -= 2;
                    }
                    var enemyPieces = Pieces.Where(p => p.Color != king.Color && p is Pawn);

                    foreach (var EnemyPawn in enemyPieces)
                    {
                        ((Pawn)EnemyPawn).EnPassantAvailable = false;
                    }

                    
                    Moves.Add($"{Colors[CurrentPlayer]} {CurrentCell.Position}");
                    PlayersMoves = Moves;

                }
                else
                {
                    IncorrectKingMoveMessage(CurrentCell, ValidMoves);

                }
            }


            return ValidMoves;
        }

        #region Incorrect move of king message
        private static void IncorrectKingMoveMessage(Cell CurrentCell, List<(int, int)> ValidMoves)
        {
            string info = "";
            foreach (var move in ValidMoves)
            {
                info += $"\t{"ABCDEFGH"[move.Item1]}{move.Item2 + 1}\n";
            }
            MessageBox.Show($"Король не может пойти на клетку: {CurrentCell.Position}\n\t Доступные ходы: \n{info}");
        }
        private static void IncorrectKingAttackMessage(Cell CurrentCell, List<(int, int)> ValidAttacks)
        {
            string info = "";
            foreach (var i in ValidAttacks)
            {
                info += $"/t{"ABCDEFGH"[i.Item1]}{i.Item2 + 1}/n";
            }
            MessageBox.Show($"Король не может атаковать клетку {CurrentCell.Position.ToString()}: \n{info}");
        }
        #endregion

        /// <summary>
        /// проверет выбрал ли игрок правильную фигуру
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        private void ChosePiece(Cell CurrentCell, Cell PreviousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PreviousActiveCell == null)
            {

                if (game.gameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece.Color == Players[CurrentPlayer % 2].Color)
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

                IPiece ChosenPiece = game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidMoves = ChosenPiece.AvailableMoves(game.GetGameField(Pieces));

                bool ShortCastleAvailble = false;

                bool LongCastleAvailable = false;

                var EnemyPieces = Pieces.Where(x => x.Color != ChosenPiece.Color).ToList();

                var MyPieces = Pieces.Where(x => x.Color == ChosenPiece.Color).ToList();

                var MyRooks = new List<Rook>();



                //Если выбранная фигура Король, то из доступных ходов нужно убрать, те которые атакованы вражескими фигурами, чтобы не было шаха
                if (ChosenPiece is King)
                {
                    var InvalidMovesCheck = ValidMoves.FindAll(cell => game.gameField.GetAtackStatus(EnemyPieces, cell, GetGameFieldString()));
                    //Убираем ходы, приводящие к шаху, из доступных ходов 
                    RemoveCheckMoves(ValidMoves, InvalidMovesCheck);

                    MyRooks = MyPieces.Where(myPiece => myPiece is Rook).Cast<Rook>().ToList();

                    IsCastlingAvailable(ValidMoves, ChosenPiece, out ShortCastleAvailble, out LongCastleAvailable, EnemyPieces, MyRooks);

                }

                if (ChosenPiece is Pawn)
                {
                    var EnemyPawn = EnemyPieces.Where(x => x.Position.Item2 == ChosenPiece.Position.Item2).Where(x => x is Pawn).Cast<Pawn>().ToList();
                    if (EnemyPawn != null)
                    {
                        ValidMoves = AddAvailableEnPassentMoves(ValidMoves, ChosenPiece, EnemyPawn);

                    }

                }

                bool IsCurrentMoveInvalid = game.gameField.GetCheckStatusAfterMove(Pieces, ChosenPiece, (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), Players[CurrentPlayer]);

                if (IsCurrentMoveInvalid)
                {
                    ValidMoves.RemoveAll(move => game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.AvailableMoves(GetGameFieldString()).Contains(move));
                }

                if (IsMoveValid(CurrentCell, ValidMoves))
                {
                    if (ShortCastlingIntention(CurrentCell, ChosenPiece, ShortCastleAvailble))
                    {
                        //короткая рокировка
                        ShortCastleModel(ChosenPiece, MyRooks);

                        ShortCastleView(PreviousActiveCell, ChosenPiece);

                        //Добавляем сделанный ход на listview в главном окне
                        //MainWindow.AddNewMove($"Короткая рокировка {ChosenPiece.Color}");
                        Moves.Add($"{ChosenPiece} 0-0");
                        PlayersMoves = Moves;
                    }
                    else if (LongCastlingIntention(CurrentCell, ChosenPiece, LongCastleAvailable))
                    {
                        //Длинная рокировка
                        LongCastleModel(ChosenPiece, MyRooks);

                        LongCastleView(PreviousActiveCell, ChosenPiece);

                        //Добавляем сделанный ход на listview в главном окне
                        //MainWindow.AddNewMove($"Длинная рокировка {ChosenPiece.Color}");
                        Moves.Add($"{ChosenPiece} 0-0-0");
                        PlayersMoves = Moves;

                    }
                    else if (EnemyPieces.Where(x => x.Position.Item2 == ChosenPiece.Position.Item2).Where(x => x is Pawn).ToList().Count > 0 && ChosenPiece is Pawn)
                    {

                        EnPassentView(CurrentCell, PreviousActiveCell, ChosenPiece);

                        EnPassentModel(CurrentCell, PreviousActiveCell);

                    }
                    else
                    {

                        //Добавляем сделанный ход на listview в главном окне
                        //MainWindow.AddNewMove(CurrentCell.Position.ToString());
                        MoveModel(CurrentCell, PreviousActiveCell);
                        //Переставляем фигуру во view
                        MoveView(CurrentCell, PreviousActiveCell);
                        //переставляем фигуру в модели

                        Moves.Add($"{ChosenPiece} {CurrentCell.Position}");
                        PlayersMoves = Moves;


                    }
                    ChangePieceProperties(CurrentCell, ChosenPiece);
                    /*
                     После любого хода, все вражеские пешки нельзя взять на проходе по правилам.
                     Для этого, после текущего хода устанавливаем у всех вражеских пешек соответсвующее значение
                    */
                    ChangeEnPassentStatusForEnimiesPawns(ChosenPiece);
                    ChangePlayer();
                }
                else
                {
                    IncorrectMoveMessage(CurrentCell, ValidMoves);

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

                //если игрок захотел поменять выбранную фигуру
                if (IsPlayerWantChoseOtherPiece(CurrentCell))
                {
                    ChoseOtherPiece(CurrentCell, PreviousActiveCell);

                }
                else//если игрок хочет съесть фигуру
                {

                    IPiece ChosenPiece = game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;
                    ValidAttacks = ChosenPiece.AvailableKills(game.GetGameField(Pieces));
                    if (ChosenPiece is King)
                    {
                        var InvalidMoves = ValidAttacks.FindAll(x => game.gameField.GetAtackStatus(Pieces.Where(x => x.Color != ChosenPiece.Color).ToList(), x, GetGameFieldString()));
                        RemoveInvalidMoves(ValidAttacks, InvalidMoves);
                    }

                    //Атаковать короля нельзя, поэтому атака короля - недоступный ход, который мы убираем из доступных ходов
                    var InvalidAttacks = ValidAttacks.FindAll(x => game.gameField[x.Item1, x.Item2].Piece is King);
                    RermoveIvalidAttacks(ValidAttacks, InvalidAttacks);

                    bool IsCurrentMoveInvalid = game.gameField.GetCheckStatusAfterMove(Pieces, ChosenPiece, (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), Players[CurrentPlayer]);

                    if (IsCurrentMoveInvalid)
                    {
                        ValidAttacks.Remove((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
                    }

                    if (IsAttackValid(CurrentCell, ValidAttacks))
                    {
                        AttackView(CurrentCell, PreviousActiveCell);
                        AttackModel(CurrentCell, PreviousActiveCell);
                        ChangePlayer();


                        Moves.Add($"{ChosenPiece} {CurrentCell.Position}");
                        PlayersMoves = Moves;
                    }
                    else
                    {
                        IncorrectAttackMessage(ValidAttacks);
                    }
                    ChangePieceProperties(CurrentCell, ChosenPiece);

                    ChangeEnPassentStatusForEnimiesPawns(ChosenPiece);
                }

            }


            return ValidAttacks;
        }

        private void ChangePlayer()
        {
            CurrentPlayer++;
            if (CurrentPlayer >= 2)
            {
                CurrentPlayer -= 2;
            }
        }

        #region Move SubMethods
        private static void IncorrectMoveMessage(Cell CurrentCell, List<(int, int)> ValidMoves)
        {
            string info = "";
            foreach (var move in ValidMoves)
            {
                info += $"\t{"ABCDEFGH"[move.Item1]}{move.Item2 + 1}\n";
            }
            MessageBox.Show($"Данная фигура не может пойти на клетку:{CurrentCell.Position}\nДоступные ходы: \n{info}");
        }

        private void ChangeEnPassentStatusForEnimiesPawns(IPiece ChosenPiece)
        {
            var enemyPieces = Pieces.Where(p => p.Color != ChosenPiece.Color && p is Pawn);

            foreach (var EnemyPawn in enemyPieces)
            {
                ((Pawn)EnemyPawn).EnPassantAvailable = false;
            }
        }

        private static void ChangePieceProperties(Cell CurrentCell, IPiece ChosenPiece)
        {
            if (IfPawnMoved2StepsForward(CurrentCell, ChosenPiece))
            {
                ((Pawn)ChosenPiece).EnPassantAvailable = true;
            }
            if (ChosenPiece is King king)
            {
                king.IsMoved = true;
            }
            if (ChosenPiece is Rook Rook)
            {
                Rook.IsMoved = true;
            }
        }

        private static bool IsMoveValid(Cell CurrentCell, List<(int, int)> ValidMoves)
        {
            return ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
        }

        private List<(int, int)> AddAvailableEnPassentMoves(List<(int, int)> ValidMoves, IPiece ChosenPiece, List<Pawn> EnemyPawn)
        {
            foreach (var pawn in EnemyPawn)
            {
                var validPawnMoves = ((Pawn)ChosenPiece).AvailableKills(game.GetGameField(Pieces), pawn);
                ValidMoves = ValidMoves.Union(validPawnMoves)?.ToList();
            }

            return ValidMoves;
        }

        private void MoveModel(Cell CurrentCell, Cell PreviousActiveCell)
        {
            game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.ChangePosition((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
        }

        private static void MoveView(Cell CurrentCell, Cell PreviousActiveCell)
        {
            PreviousActiveCell.Active = false;
            CurrentCell.State = PreviousActiveCell.State;
            PreviousActiveCell.State = State.Empty;
        }

        private static bool IfPawnMoved2StepsForward(Cell CurrentCell, IPiece ChosenPiece)
        {
            return ChosenPiece is Pawn &&
                (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (((Pawn)ChosenPiece).StartPos.Item1 + ((Pawn)ChosenPiece).MoveDir[1].Item1, ((Pawn)ChosenPiece).StartPos.Item2 + ((Pawn)ChosenPiece).MoveDir[1].Item2);
        }

        private void EnPassentModel(Cell CurrentCell, Cell PreviousActiveCell)
        {
            game.gameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.ChangePosition((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
        }

        private void EnPassentView(Cell CurrentCell, Cell PreviousActiveCell, IPiece ChosenPiece)
        {
            PreviousActiveCell.Active = false;
            CurrentCell.State = PreviousActiveCell.State;
            PreviousActiveCell.State = State.Empty;

            Board[7 - ChosenPiece.Position.Item2, CurrentCell.Position.Horizontal] = State.Empty;
        }

        private static void RemoveCheckMoves(List<(int, int)> ValidMoves, List<(int, int)> InvalidMovesCheck)
        {
            foreach (var move in InvalidMovesCheck)
            {
                ValidMoves.Remove(move);
            }
        }

        private void IsCastlingAvailable(List<(int, int)> ValidMoves, IPiece ChosenPiece, out bool ShortCastleAvailble, out bool LongCastleAvailable, List<IPiece> EnemyPieces, List<Rook> MyRooks)
        {
            ShortCastleAvailble = ((King)ChosenPiece).ShortCastling(MyRooks.Where(rook => rook.RookKind == RookKind.Royal).ToList()[0], game.gameField, EnemyPieces, GetGameFieldString());
            LongCastleAvailable = ((King)ChosenPiece).LongCastling(MyRooks.Where(rook => rook.RookKind == RookKind.Queen).ToList()[0], game.gameField, EnemyPieces, GetGameFieldString());
            //Если рокировки доступны, то добавляем их в список доступных ходов
            if (ShortCastleAvailble)
            {
                ValidMoves.Add((ShortCastleVerticalPosition, ChosenPiece.Position.Item2));
            }

            if (LongCastleAvailable)
            {
                ValidMoves.Add((LongCastleVerticalPosition, ChosenPiece.Position.Item2));
            }
        }

        private static void LongCastleModel(IPiece ChosenPiece, List<Rook> MyRooks)
        {
            ((King)ChosenPiece).ChangePosition((2, ChosenPiece.Position.Item2));

            Rook MyQueenRook = MyRooks.Where(MyRook => MyRook.RookKind == RookKind.Queen).ToList()[0];

            MyQueenRook.ChangePosition((3, MyQueenRook.Position.Item2));
        }

        private static void ShortCastleModel(IPiece ChosenPiece, List<Rook> MyRooks)
        {
            ((King)ChosenPiece).ChangePosition((6, ChosenPiece.Position.Item2));

            Rook MyRoyalRook = MyRooks.Where(MyRook => MyRook.RookKind == RookKind.Royal).ToList()[0];

            MyRoyalRook.ChangePosition((5, MyRoyalRook.Position.Item2));
        }

        private static bool LongCastlingIntention(Cell CurrentCell, IPiece piece, bool LongCastleAvailable)
        {
            return LongCastleAvailable && piece is King && (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (LongCastleVerticalPosition, piece.Position.Item2);
        }

        private static bool ShortCastlingIntention(Cell CurrentCell, IPiece piece, bool ShortCastleAvailable)
        {
            return ShortCastleAvailable && piece is King && (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (ShortCastleVerticalPosition, piece.Position.Item2);
        }

        /// <summary>
        /// Отрисовка длинной рокировки 
        /// </summary>
        /// <param name="PreviousActiveCell">Предыдущая выделенная клетка</param>
        /// <param name="piece">Выбранная фигура</param>
        private void LongCastleView(Cell PreviousActiveCell, IPiece piece)
        {
            /*
            При обращении к Board через индексы важно помнить, что позиция по горизонтали у Board - вторая координата, позиция по вертикали - первая.
            Порядок по вертикали сверху вниз, то есть белый король находится на 7 строке у Board
            */
            PreviousActiveCell.Active = false;
            Board[7 - piece.Position.Item2, 2] = PreviousActiveCell.State;
            PreviousActiveCell.State = State.Empty;
            Board[7 - piece.Position.Item2, 3] = Board[7 - piece.Position.Item2, 0];
            Board[7 - piece.Position.Item2, 0] = State.Empty;
        }

        /// <summary>
        /// Отрисовка короткой рокировки
        /// </summary>
        /// <param name="PreviousActiveCell">предыдущая выделенная клетка</param>
        /// <param name="piece">Выбранная фигура</param>
        private void ShortCastleView(Cell PreviousActiveCell, IPiece piece)
        {
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
        #endregion
        #region Attack SubMethods

        private static void IncorrectAttackMessage(List<(int, int)> ValidAttacks)
        {
            string AttackInfo = "";
            foreach (var validAttack in ValidAttacks)
            {
                AttackInfo += $"{"ABCDEFGH"[validAttack.Item1]}{validAttack.Item2 + 1}\n";
            }
            MessageBox.Show($"Съесть нельзя! Доступные клетки для атаки\n:{AttackInfo}");
        }

        private void AttackModel(Cell CurrentCell, Cell PreviousActiveCell)
        {
            game.CheckIfPieceWasKilled((PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical), (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), GetGameFieldString(), Pieces);
            game.RemoveDeadPieces(Pieces);
        }

        private static void AttackView(Cell CurrentCell, Cell PreviousActiveCell)
        {
            CurrentCell.State = PreviousActiveCell.State;
            PreviousActiveCell.Active = false;
            PreviousActiveCell.State = State.Empty;
        }

        private static bool IsAttackValid(Cell CurrentCell, List<(int, int)> ValidAttacks)
        {
            return ValidAttacks.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
        }

        private static void RermoveIvalidAttacks(List<(int, int)> ValidAttacks, List<(int, int)> InvalidAttacks)
        {
            foreach (var move in InvalidAttacks)
            {
                ValidAttacks.Remove(move);
            }
        }

        private static void RemoveInvalidMoves(List<(int, int)> ValidAttacks, List<(int, int)> InvalidMoves)
        {
            foreach (var move in InvalidMoves)
            {
                ValidAttacks.Remove(move);
            }
        }

        private static void ChoseOtherPiece(Cell CurrentCell, Cell PreviousActiveCell)
        {
            PreviousActiveCell.Active = !PreviousActiveCell.Active;
            CurrentCell.Active = true;
        }

        private bool IsPlayerWantChoseOtherPiece(Cell CurrentCell)
        {
            return game.gameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece.Color == Players[CurrentPlayer % 2].Color;
        }
        #endregion

        /// <summary>
        /// Утсанавливает начальные позиции фигурам при старте игры, для WPF
        /// </summary>
        private void SetupBoard()
        {
            CurrentPlayer = 0;
            Pieces = game.GetPiecesStartPosition();
            Players = new List<Player>()
        {
            new Player(ChessLib.PieceColor.White,Pieces.Where(x=> x.Color == ChessLib.PieceColor.White).ToList(),"user1"),
            new Player(ChessLib.PieceColor.Black,Pieces.Where(x=> x.Color == ChessLib.PieceColor.Black).ToList(),"user2")
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
            playerMoves = new ObservableCollection<string>();
        }
       
        public MainViewModel()
        {
            
            
        }
        private string[,] GetGameFieldString()
        {
            string[,] result = game.GetGameField(Pieces);
            return result;
        }
    }
}