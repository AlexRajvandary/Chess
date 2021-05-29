using System;
using System.Collections.Generic;

namespace ChessLib
{

    public class Pawn : IPiece
    {
        public bool IsDead { get; set; }
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }

        /// <summary>
        /// Стартовая позиция, нужна для проверки доступных ходов у пешки (если пешка в начальной позиции, то существует 2 варианта хода)
        /// </summary>
        private (int, int) startPos;

        /// <summary>
        /// проверяет все доступные ходы для текущей пешки
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns></returns>
        public List<(int, int)> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<(int, int)>();

            if (Position == startPos)
            {
                if (Color == PieceColor.White)
                {
                    if (GameField[Position.Item1, Position.Item2 + 1] != " ")
                    {
                        Console.WriteLine("Нет свободных ходов!");
                        return AvailableMovesList;
                    }
                    else
                    {
                        AvailableMovesList.Add((Position.Item1, Position.Item2 + 1));
                        if (GameField[Position.Item1, Position.Item2 + 2] != " ")
                        {
                            return AvailableMovesList;
                        }
                        else
                        {
                            AvailableMovesList.Add((Position.Item1, Position.Item2 + 2));
                            return AvailableMovesList;
                        }
                    }
                }
                else
                {
                    if (GameField[Position.Item1, Position.Item2 - 1] != " ")
                    {
                        Console.WriteLine("Нет свободных ходов!");
                        return AvailableMovesList;
                    }
                    else
                    {
                        AvailableMovesList.Add((Position.Item1, Position.Item2 - 1));
                        if (GameField[Position.Item1, Position.Item2 - 2] != " ")
                        {
                            return AvailableMovesList;
                        }
                        else
                        {
                            AvailableMovesList.Add((Position.Item1, Position.Item2 - 2));
                            return AvailableMovesList;
                        }
                    }
                }

            }
            else
            {

                if (Color == PieceColor.White)
                {
                    if (GameField[Position.Item1, Position.Item2 + 1] != " ")
                    {
                        return AvailableMovesList;
                    }
                    else
                    {
                        AvailableMovesList.Add((Position.Item1, Position.Item2 + 1));
                        return AvailableMovesList;
                    }
                }
                else
                {
                    if (GameField[Position.Item1, Position.Item2 - 1] != " ")
                    {
                        return AvailableMovesList;
                    }
                    else
                    {
                        AvailableMovesList.Add((Position.Item1, Position.Item2 - 1));
                        return AvailableMovesList;
                    }
                }

            }
        }

        public override string ToString()
        {
            return "p";
        }
        /// <summary>
        /// Направления для атаки
        /// </summary>
        private readonly (int, int)[] Directions = new (int, int)[] { (-1, 1), (1, 1), (-1, -1), (1, -1) };
        /// <summary>
        /// Условия для проверки возможности атаки
        /// </summary>
        private readonly Func<int, int, bool>[] Conditions = new Func<int, int, bool>[]
        {
            (x,y)=> x>0 && y<7,
            (x,y)=> x<7 && y<7,
            (x,y)=> x>0 && y>0,
            (x,y)=> x<7 && y>0

        };
        /// <summary>
        /// Ищет доступные для атаки вражеские фигуры
        /// </summary>
        /// <param name="GameField">Игровое поле</param>
        /// <returns>Возвращает список координат доступных для атаки фигур</returns>
        public List<(int, int)> AvailableKills(string[,] GameField)
        {
            var AvailableKillsList = new List<(int, int)>();

            GetOppositeAndFriendPieces();
            for (int i = 0; i < 4; i++)
            {

                if (Conditions[i](Position.Item1, Position.Item2))
                {

                    if (GameField[Position.Item1 + Directions[i].Item1, Position.Item2 + Directions[i].Item2] != " " && pieces.Contains(GameField[Position.Item1 + Directions[i].Item1, Position.Item2 + Directions[i].Item2]))
                    {
                        AvailableKillsList.Add((Position.Item1 + Directions[i].Item1, Position.Item2 + Directions[i].Item2));
                    }
                }

            }

            return AvailableKillsList;

        }
        private string pieces;

        private void GetOppositeAndFriendPieces()
        {

            if (Color == PieceColor.White)
            {
                pieces = "kbnpqr";
            }
            else
            {
                pieces = "KBNPQR";
            }


        }

        public Pawn(PieceColor color, (int, int) position)
        {
            Color = color;
            startPos = position;
            Position = startPos;
            IsDead = false;
        }
    }
}
