﻿using ChessLib;
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
        private ICommand _clearCommand;
        private ICommand _cellCommand;
        List<Player> players;

        private Game game;
        private List<IPiece> pieces;

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

            Cell CurrentCell = (Cell)parameter;//текущая активная клетка, вернее cell.active у нее false сейчас, но эта именно та клетка на которую мы сейчас нажали

            List<(int, int)> ValidMoves = new List<(int, int)>();//доступные ходы для текущей фигуры

            List<(int, int)> ValidAtacks = new List<(int, int)>();//доступные атаки для текущей фигуры

            Cell PriveousActiveCell = Board.FirstOrDefault(x => x.Active);//предыдущая активная клетка, на которую нажали до текущей (т.е. как бы в первый раз мы выбрали фигуру, а потом, когда нажали на вторую клетку, выбрали куда пойти )

            if (game.gameField.IsCheck())
            {
                IsKingWasChosed(CurrentCell, PriveousActiveCell);
                //атака королем под шахом
                ValidAtacks = KingAttackCheck(CurrentCell, ValidAtacks, PriveousActiveCell);
                //ход королем под шахом
                ValidMoves = KingMoveCheck(CurrentCell, ValidMoves, PriveousActiveCell);
            }
            else
            {

                ChosePiece(CurrentCell, PriveousActiveCell);
                ValidAtacks = Attack(CurrentCell, ValidAtacks, PriveousActiveCell);
                ValidMoves = Move(CurrentCell, ValidMoves, PriveousActiveCell);
                game.gameField.Update(pieces, getGameFieldString(), players[currentPlayer % 2].Color);
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
        /// <param name="PriveousActiveCell">предыдущая клетка</param>
        private void IsKingWasChosed(Cell CurrentCell, Cell PriveousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PriveousActiveCell is null)
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
        /// <param name="ValidAtacks">Доступные клетки для атаки</param>
        /// <param name="PriveousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private List<(int, int)> KingAttackCheck(Cell CurrentCell, List<(int, int)> ValidAtacks, Cell PriveousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PriveousActiveCell != null)
            {
                game.Update(pieces);
                game.gameField.Update(pieces, getGameFieldString(), players[currentPlayer % 2].Color);
                CurrentCell.Active = true;
                PriveousActiveCell.Active = false;
                King king = (King)game.gameField[PriveousActiveCell.Position.Horizontal, PriveousActiveCell.Position.Vertical].Piece;

                ValidAtacks = king.AvailableKills(game.GetGameField(pieces));
                if (ValidAtacks.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    CurrentCell.State = PriveousActiveCell.State;
                    PriveousActiveCell.Active = false;
                    PriveousActiveCell.State = State.Empty;
                    game.CheckIfPieceWasKilled((PriveousActiveCell.Position.Horizontal, PriveousActiveCell.Position.Vertical), (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), getGameFieldString(), pieces);
                    game.Update(pieces);
                    currentPlayer++;
                    if (currentPlayer >= 2)
                    {
                        currentPlayer -= 2;
                    }
                }
                else
                {
                    string info = "";
                    foreach (var i in ValidAtacks)
                    {
                        info += $"/t{"ABCDEFGH"[i.Item1]}{i.Item2 + 1}/n";
                    }
                    MessageBox.Show($"Что-то не так: \n{info}");


                }
            }

            return ValidAtacks;
        }
        /// <summary>
        /// Ход королем под шахом
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="ValidMoves">Доступные клетки для хода</param>
        /// <param name="PriveousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private List<(int, int)> KingMoveCheck(Cell CurrentCell, List<(int, int)> ValidMoves, Cell PriveousActiveCell)
        {
            if (CurrentCell.State == State.Empty && PriveousActiveCell != null)
            {
                game.Update(pieces);
                game.gameField.Update(pieces, getGameFieldString(), players[currentPlayer % 2].Color);
                CurrentCell.Active = true;
                PriveousActiveCell.Active = false;
                King king = (King)game.gameField[PriveousActiveCell.Position.Horizontal, PriveousActiveCell.Position.Vertical].Piece;

                ValidMoves = king.AvailableMoves(game.GetGameField(pieces));
                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {
                    PriveousActiveCell.Active = false;
                    CurrentCell.State = PriveousActiveCell.State;
                    PriveousActiveCell.State = State.Empty;
                    game.gameField[PriveousActiveCell.Position.Horizontal, PriveousActiveCell.Position.Vertical].Piece.Position = (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical);//переставляем фигуру в модели

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
                    MessageBox.Show($"Что-то не так,текущий ход:{"ABCDEFGH"[CurrentCell.Position.Horizontal]}{CurrentCell.Position.Vertical + 1}\n\t Доступные ходы: \n{info}");

                }
            }

            return ValidMoves;
        }
        /// <summary>
        /// проверет выбрал ли игрок правильную фигуру
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="PriveousActiveCell">Предыдущая клетка</param>
        private void ChosePiece(Cell CurrentCell, Cell PriveousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PriveousActiveCell == null)
            {

                game.gameField.Update(pieces, getGameFieldString(), players[currentPlayer % 2].Color);


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
        /// <param name="PriveousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private List<(int, int)> Move(Cell CurrentCell, List<(int, int)> ValidMoves, Cell PriveousActiveCell)
        {
            if (CurrentCell.State == State.Empty && PriveousActiveCell != null)
            {
                game.gameField.Update(pieces, getGameFieldString(), players[currentPlayer % 2].Color);
                IPiece piece = game.gameField[PriveousActiveCell.Position.Horizontal, PriveousActiveCell.Position.Vertical].Piece;
                ValidMoves = piece.AvailableMoves(game.GetGameField(pieces));


                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {
                    PriveousActiveCell.Active = false;
                    CurrentCell.State = PriveousActiveCell.State;
                    PriveousActiveCell.State = State.Empty;
                    game.gameField[PriveousActiveCell.Position.Horizontal, PriveousActiveCell.Position.Vertical].Piece.Position = (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical);//переставляем фигуру в модели

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
                        info += $"{"ABCDEFGH"[move.Item1]}{move.Item2 + 1}\n";
                    }
                    MessageBox.Show($"Что-то не так,текущий ход:{"ABCDEFGH"[CurrentCell.Position.Horizontal]}{CurrentCell.Position.Vertical + 1} доступные ходы: \n{info}");
                }




            }

            return ValidMoves;
        }
        /// <summary>
        /// Проверяет можно ли атаковать и атакует, либо выводит сообщение об ошибке
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="ValidAtacks">Доступные клетки для атаки</param>>
        /// <param name="PriveousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private List<(int, int)> Attack(Cell CurrentCell, List<(int, int)> ValidAtacks, Cell PriveousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PriveousActiveCell != null)
            {

                game.gameField.Update(pieces, getGameFieldString(), players[currentPlayer % 2].Color);

                //если игрок захотел поменять выбранную фигуру
                if (game.gameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece.Color == players[currentPlayer % 2].Color)
                {
                    PriveousActiveCell.Active = !PriveousActiveCell.Active;
                    CurrentCell.Active = true;

                }
                else//если игрок хочет съесть фигуру
                {
                    game.gameField.Update(pieces, getGameFieldString(), players[currentPlayer % 2].Color);
                    IPiece piece = game.gameField[PriveousActiveCell.Position.Horizontal, PriveousActiveCell.Position.Vertical].Piece;
                    ValidAtacks = piece.AvailableKills(game.GetGameField(pieces));

                    if (ValidAtacks.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                    {
                        CurrentCell.State = PriveousActiveCell.State;
                        PriveousActiveCell.Active = false;
                        PriveousActiveCell.State = State.Empty;
                        game.CheckIfPieceWasKilled((PriveousActiveCell.Position.Horizontal, PriveousActiveCell.Position.Vertical), (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), getGameFieldString(), pieces);
                        game.Update(pieces);
                        currentPlayer++;
                        if (currentPlayer >= 2)
                        {
                            currentPlayer -= 2;
                        }
                    }
                    else
                    {
                        string AttackInfo = "";
                        foreach (var validAttack in ValidAtacks)
                        {
                            AttackInfo += $"{"ABCDEFGH"[validAttack.Item1]}{validAttack.Item2 + 1}\n";
                        }
                        MessageBox.Show($"Съесть нельзя! Доступные клетки для атаки\n:{AttackInfo}");
                    }


                }



            }

            return ValidAtacks;
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
        }

        public MainViewModel()
        {


        }
        private string[,] getGameFieldString()
        {
            string[,] result = game.GetGameField(pieces);
            return result;
        }
    }
}