using System.Collections.Generic;
using System;

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
        /// Ищет доступные для атаки вражеские фигуры
        /// </summary>
        /// <param name="GameField">Игровое поле</param>
        /// <returns>Возвращает список координат доступных для атаки фигур</returns>
        public List<(int, int)> AvailableKills(string[,] GameField)
        {
            var AvailableKillsList = new List<(int, int)>();

            string pieces;
            if (Color == PieceColor.White)
            {
                pieces = "bnpqr";
            }
            else
            {
                pieces = "BNPQR";
            }

            if (Color == PieceColor.White)
            {
                if (Position.Item1 > 0 && Position.Item2 < 7)
                {
                    if (Position.Item1 > 0 && Position.Item2 < 7)
                        if (GameField[Position.Item1 - 1, Position.Item2 + 1] != " " && pieces.Contains(GameField[Position.Item1 - 1, Position.Item2 + 1]))
                        {
                            AvailableKillsList.Add((Position.Item1 - 1, Position.Item2 + 1));
                        }
                }
                if (Position.Item1 < 7 && Position.Item2 < 7)
                {
                    if (Position.Item1 > 0 && Position.Item2 < 7)
                        if (GameField[Position.Item1 + 1, Position.Item2 + 1] != " " && pieces.Contains(GameField[Position.Item1 + 1, Position.Item2 + 1]))
                        {
                            AvailableKillsList.Add((Position.Item1 + 1, Position.Item2 + 1));
                        }
                }

                return AvailableKillsList;
            }
            else
            {
                if (Position.Item1 > 0 && Position.Item2 > 0)
                {
                    if (GameField[Position.Item1 - 1, Position.Item2 - 1] != " " && pieces.Contains(GameField[Position.Item1 - 1, Position.Item2 - 1]))
                    {
                        AvailableKillsList.Add((Position.Item1 - 1, Position.Item2 - 1));
                    }
                }
                if (Position.Item1 < 7 && Position.Item2 > 0)
                {
                    if (GameField[Position.Item1 + 1, Position.Item2 - 1] != " " && pieces.Contains(GameField[Position.Item1 + 1, Position.Item2 - 1]))
                    {
                        AvailableKillsList.Add((Position.Item1 + 1, Position.Item2 - 1));
                    }
                }
                return AvailableKillsList;
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
