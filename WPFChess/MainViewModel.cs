using ChessLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ChessBoard
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private const int _longCastleVerticalPosition = 2;

        private const int _shortCastleVerticalPosition = 6;

        private int _currentPlayer;

        private Board _board = new Board();

        private ICommand _newGameCommand;

        private ICommand _cellCommand;
        private List<Player> _players;
        private readonly List<string> _colors = new List<string>() { "Белые", "Черные" };

        private Game _game;

        private List<IPiece> _pieces;

        private ObservableCollection<string> _moves;

        private ObservableCollection<string> _playerMoves;

        private string pathOfFenFile = "fen.txt";

        public ObservableCollection<string> PlayersMoves
        {
            get => _playerMoves;
            set
            {
                _playerMoves = value;
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

        public ICommand NewGameCommand => _newGameCommand ??= new RelayCommand(parameter =>
        {
            _game = new Game();
            _playerMoves = new ObservableCollection<string>();
            _moves = new ObservableCollection<string>();
            File.WriteAllText(pathOfFenFile, $"{DateTime.Now.ToString()}\n");
            Fen.GameField = _game.GameField;

            SetupBoard();
        });


        public ICommand CellCommand => _cellCommand ??= new RelayCommand(parameter =>
        {

            Cell CurrentCell = (Cell)parameter;

            Cell PreviousActiveCell = Board.FirstOrDefault(x => x.Active);

            List<(int, int)> ValidMoves = new();

            List<(int, int)> ValidAttacks = new();


            if (IsCheck())
            {
                UpdateModel();

                IsKingWasChosed(CurrentCell, PreviousActiveCell);

                //атака королем под шахом
                ValidAttacks = KingAttackCheck(CurrentCell, ValidAttacks, PreviousActiveCell);

                //ход королем под шахом
                ValidMoves = KingMoveCheck(CurrentCell, ValidMoves, PreviousActiveCell);

                UpdateModel();
            }
            else
            {
                UpdateModel();

                ChosePiece(CurrentCell, PreviousActiveCell);

                //Игрок хочет атаковать
                ValidAttacks = Attack(CurrentCell, ValidAttacks, PreviousActiveCell);

                //Игрок хочет сделать ход
                ValidMoves = Move(CurrentCell, ValidMoves, PreviousActiveCell);

                UpdateModel();

                CheckMessage();

                File.AppendAllText(pathOfFenFile, $"{Fen.GetFenFromTheGameField()}\n");
            }

        }, parameter => parameter is Cell cell && (Board.Any(x => x.Active) || cell.State != State.Empty));

        #region CellCommand SubMethods
        private void CheckMessage()
        {
            if (IsCheck())
            {
                MessageBox.Show("Шах!");
            }
        }

        private bool IsCheck()
        {
            return _game.GameField.IsCheck();
        }

        /// <summary>
        /// Убирает убитые фигуры, обновяет доску
        /// </summary>
        private void UpdateModel()
        {
            _game.RemoveDeadPieces(_pieces);
            _game.GameField.Update(_pieces, GetGameFieldString(), _players[_currentPlayer % 2].Color);
        }
        #endregion



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

                if (_game.GameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece is King)
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
                King king = (King)_game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidAttacks = king.AvailableKills(_game.GetGameField(_pieces));

                //Проверяем атакована ли клетка, на которую собирается пойти король
                var AvailableAttacksForKingInCheck = ValidAttacks.FindAll(x => _game.GameField.GetAtackStatus(_pieces.Where(x => x.Color != king.Color).ToList(), x, GetGameFieldString()));
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

                    MakeEnPassentUnavailableForAllPawns();


                    //MainWindow.AddNewMove(CurrentCell.Position.ToString());
                    _moves.Add($"{_players[_currentPlayer].Color} {CurrentCell.Position}");
                    PlayersMoves = _moves;

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
                King king = (King)_game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidMoves = king.AvailableMoves(_game.GetGameField(_pieces));
                var EnemyPieces = _pieces.Where(x => x.Color != king.Color).ToList();
                var AvailableMovesForKingInCheck = ValidMoves.FindAll(x => _game.GameField.GetAtackStatus(EnemyPieces, x, GetGameFieldString()));
                foreach (var removedMoves in AvailableMovesForKingInCheck)
                {
                    ValidMoves.Remove(removedMoves);
                }

                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    king.IsMoved = true;

                    MoveView(CurrentCell, PreviousActiveCell);

                    MoveModel(CurrentCell, PreviousActiveCell);

                    MakeEnPassentUnavailableForAllPawns();

                    AddCurrentMoveToListView(CurrentCell);

                    ChangePlayer();

                }
                else
                {
                    IncorrectKingMoveMessage(CurrentCell, ValidMoves);
                }
            }


            return ValidMoves;
        }
        /// <summary>
        ///  После любого хода, все вражеские пешки нельзя взять на проходе по правилам.
        /// Для этого, после текущего хода устанавливаем у всех вражеских пешек соответсвующее значение
        /// </summary>
        private void MakeEnPassentUnavailableForAllPawns()
        {
            var enemyPawns = _pieces.Where(p => p is Pawn);

            foreach (var EnemyPawn in enemyPawns)
            {
                ((Pawn)EnemyPawn).EnPassantAvailable = false;
            }
        }

        private void AddCurrentMoveToListView(Cell CurrentCell)
        {
            _moves.Add($"{_colors[_currentPlayer]} {CurrentCell.Position}");
            PlayersMoves = _moves;
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

                if (_game.GameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece.Color == _players[_currentPlayer % 2].Color)
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
            if (IsPlayerGoingToEmptyCell(CurrentCell, PreviousActiveCell))
            {

                IPiece ChosenPiece = _game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidMoves = ChosenPiece.AvailableMoves(_game.GetGameField(_pieces));

                bool ShortCastleAvailble = false;

                bool LongCastleAvailable = false;

                var EnemyPieces = _pieces.Where(x => x.Color != ChosenPiece.Color).ToList();

                var MyPieces = _pieces.Where(x => x.Color == ChosenPiece.Color).ToList();

                var MyRooks = new List<Rook>();

                IfPieceIsKingRemoveAttackedCells(ValidMoves, ChosenPiece, ref ShortCastleAvailble, ref LongCastleAvailable, EnemyPieces, MyPieces, ref MyRooks);

                ValidMoves = IfChosenPieceIsPawnAddValidEnPassentMoves(ValidMoves, ChosenPiece, EnemyPieces);

                RemoveIncorrectMove(CurrentCell, ValidMoves, PreviousActiveCell, ChosenPiece);

                if (IsMoveValid(CurrentCell, ValidMoves))
                {
                    if (ShortCastlingIntention(CurrentCell, ChosenPiece, ShortCastleAvailble))
                    {
                        ShortCastleModel(ChosenPiece, MyRooks);

                        ShortCastleView(PreviousActiveCell, ChosenPiece);

                        _moves.Add($"{ChosenPiece} 0-0");
                        PlayersMoves = _moves;
                    }
                    else if (LongCastlingIntention(CurrentCell, ChosenPiece, LongCastleAvailable))
                    {
                        LongCastleModel(ChosenPiece, MyRooks);

                        LongCastleView(PreviousActiveCell, ChosenPiece);

                        //Добавляем сделанный ход на listview в главном окне
                        //MainWindow.AddNewMove($"Длинная рокировка {ChosenPiece.Color}");
                        _moves.Add($"{ChosenPiece} 0-0-0");
                        PlayersMoves = _moves;
                    }
                    else if (EnemyPieces.Where(x => x.Position.Item2 == ChosenPiece.Position.Item2).Where(x => x is Pawn).ToList().Count > 0 && ChosenPiece is Pawn)
                    {
                        EnPassentView(CurrentCell, PreviousActiveCell, ChosenPiece);

                        EnPassentModel(CurrentCell, PreviousActiveCell);

                        AddCurrentMoveToListView(CurrentCell);
                    }
                    else
                    {
                        MoveModel(CurrentCell, PreviousActiveCell);

                        MoveView(CurrentCell, PreviousActiveCell);

                        AddCurrentMoveToListView(CurrentCell);
                    }
                    ChangePieceProperties(CurrentCell, ChosenPiece);

                    MakeEnPassentUnavailableForAllPawns();

                    ChangePlayer();
                }
                else
                {
                    IncorrectMoveMessage(CurrentCell, ValidMoves);

                }

            }

            return ValidMoves;
        }

        private static bool IsPlayerGoingToEmptyCell(Cell CurrentCell, Cell PreviousActiveCell)
        {
            return CurrentCell.State == State.Empty && PreviousActiveCell != null;
        }

        private void RemoveIncorrectMove(Cell CurrentCell, List<(int, int)> ValidMoves, Cell PreviousActiveCell, IPiece ChosenPiece)
        {
            if (IsCurrentMoveMakeCheckToOurKing(CurrentCell, ChosenPiece))
            {
                ValidMoves.RemoveAll(move => _game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.AvailableMoves(GetGameFieldString()).Contains(move));
            }
        }

        private void IfPieceIsKingRemoveAttackedCells(List<(int, int)> ValidMoves, IPiece ChosenPiece, ref bool ShortCastleAvailble, ref bool LongCastleAvailable, List<IPiece> EnemyPieces, List<IPiece> MyPieces, ref List<Rook> MyRooks)
        {
            if (ChosenPiece is King)
            {
                var InvalidMovesForKing = FindsAttackedCells(ValidMoves, EnemyPieces);

                RemoveCheckMoves(ValidMoves, InvalidMovesForKing);

                MyRooks = GetMyRooks(MyPieces);

                IsCastlingAvailable(ValidMoves, ChosenPiece, out ShortCastleAvailble, out LongCastleAvailable, EnemyPieces, MyRooks);

            }
        }

        private List<(int, int)> IfChosenPieceIsPawnAddValidEnPassentMoves(List<(int, int)> ValidMoves, IPiece ChosenPiece, List<IPiece> EnemyPieces)
        {
            if (ChosenPiece is Pawn)
            {
                var EnemyPawn = EnemyPieces.Where(x => x.Position.Item2 == ChosenPiece.Position.Item2).Where(x => x is Pawn).Cast<Pawn>().ToList();

                if (EnemyPawn != null)
                {
                    ValidMoves = AddAvailableEnPassentMoves(ValidMoves, ChosenPiece, EnemyPawn);

                }

            }

            return ValidMoves;
        }

        private List<(int, int)> FindsAttackedCells(List<(int, int)> ValidMoves, List<IPiece> EnemyPieces)
        {
            return ValidMoves.FindAll(cell => _game.GameField.GetAtackStatus(EnemyPieces, cell, GetGameFieldString()));
        }

        private static List<Rook> GetMyRooks(List<IPiece> MyPieces)
        {
            return MyPieces.Where(myPiece => myPiece is Rook).Cast<Rook>().ToList();
        }

        private bool IsCurrentMoveMakeCheckToOurKing(Cell CurrentCell, IPiece ChosenPiece)
        {
            return _game.GameField.GetCheckStatusAfterMove(_pieces, ChosenPiece, (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), _players[_currentPlayer]);
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

                    IPiece ChosenPiece = _game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;
                    ValidAttacks = ChosenPiece.AvailableKills(_game.GetGameField(_pieces));
                    if (ChosenPiece is King)
                    {
                        var InvalidMoves = ValidAttacks.FindAll(x => _game.GameField.GetAtackStatus(_pieces.Where(x => x.Color != ChosenPiece.Color).ToList(), x, GetGameFieldString()));
                        RemoveInvalidMoves(ValidAttacks, InvalidMoves);
                    }

                    //Атаковать короля нельзя, поэтому атака короля - недоступный ход, который мы убираем из доступных ходов
                    var InvalidAttacks = ValidAttacks.FindAll(x => _game.GameField[x.Item1, x.Item2].Piece is King);
                    RermoveIvalidAttacks(ValidAttacks, InvalidAttacks);

                    bool IsCurrentMoveInvalid = _game.GameField.GetCheckStatusAfterMove(_pieces, ChosenPiece, (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), _players[_currentPlayer]);

                    if (IsCurrentMoveInvalid)
                    {
                        ValidAttacks.Remove((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
                    }

                    if (IsAttackValid(CurrentCell, ValidAttacks))
                    {
                        AttackView(CurrentCell, PreviousActiveCell);
                        AttackModel(CurrentCell, PreviousActiveCell);
                        ChangePlayer();


                        _moves.Add($"{ChosenPiece} {CurrentCell.Position}");
                        PlayersMoves = _moves;
                    }
                    else
                    {
                        IncorrectAttackMessage(ValidAttacks);
                    }
                    ChangePieceProperties(CurrentCell, ChosenPiece);

                    MakeEnPassentUnavailableForAllPawns();
                }

            }

            return ValidAttacks;
        }

        private void ChangePlayer()
        {
            _currentPlayer++;
            if (_currentPlayer >= 2)
            {
                _currentPlayer -= 2;
            }
            Fen.CurrentPlayer = _currentPlayer;
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
                var validPawnMoves = ((Pawn)ChosenPiece).AvailableKills(_game.GetGameField(_pieces), pawn);
                ValidMoves = ValidMoves.Union(validPawnMoves)?.ToList();
            }

            return ValidMoves;
        }

        private void MoveModel(Cell CurrentCell, Cell PreviousActiveCell)
        {
            _game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.ChangePosition((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
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
            _game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.ChangePosition((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
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
            ShortCastleAvailble = ((King)ChosenPiece).ShortCastling(MyRooks.Where(rook => rook.RookKind == RookKind.Royal).ToList()[0], _game.GameField, EnemyPieces, GetGameFieldString());
            LongCastleAvailable = ((King)ChosenPiece).LongCastling(MyRooks.Where(rook => rook.RookKind == RookKind.Queen).ToList()[0], _game.GameField, EnemyPieces, GetGameFieldString());
            //Если рокировки доступны, то добавляем их в список доступных ходов
            if (ShortCastleAvailble)
            {
                ValidMoves.Add((_shortCastleVerticalPosition, ChosenPiece.Position.Item2));
            }

            if (LongCastleAvailable)
            {
                ValidMoves.Add((_longCastleVerticalPosition, ChosenPiece.Position.Item2));
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
            return LongCastleAvailable && piece is King && (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (_longCastleVerticalPosition, piece.Position.Item2);
        }

        private static bool ShortCastlingIntention(Cell CurrentCell, IPiece piece, bool ShortCastleAvailable)
        {
            return ShortCastleAvailable && piece is King && (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (_shortCastleVerticalPosition, piece.Position.Item2);
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
            _game.CheckIfPieceWasKilled((PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical), (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), GetGameFieldString(), _pieces);
            _game.RemoveDeadPieces(_pieces);
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
            return _game.GameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece.Color == _players[_currentPlayer % 2].Color;
        }
        #endregion

        /// <summary>
        /// Утсанавливает начальные позиции фигурам при старте игры, для WPF
        /// </summary>
        private void SetupBoard()
        {
            _currentPlayer = 0;
            _pieces = _game.GetPiecesStartPosition();
            _players = new List<Player>()
        {
            new Player(ChessLib.PieceColor.White,_pieces.Where(x=> x.Color == ChessLib.PieceColor.White).ToList(),"user1"),
            new Player(ChessLib.PieceColor.Black,_pieces.Where(x=> x.Color == ChessLib.PieceColor.Black).ToList(),"user2")
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
            _playerMoves = new ObservableCollection<string>();
        }

        public MainViewModel()
        {


        }
        private string[,] GetGameFieldString()
        {
            string[,] result = _game.GetGameField(_pieces);
            return result;
        }
    }
}