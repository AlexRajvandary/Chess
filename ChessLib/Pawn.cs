using System.Collections.Generic;
using System;

namespace ChessLib
{

    public class Pawn : IPiece
    {
        public bool Dead { get; set; }
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
        public List<(int, int)> AvalableMoves(string[,] GameField)
        {
            var result = new List<(int, int)>();
            Console.WriteLine("Мы здесь");
            if (Position == startPos)
            {
                if (Color == PieceColor.White)
                {
                    if (GameField[Position.Item1, Position.Item2 + 1] != null)
                    {
                        Console.WriteLine("Нет свободных ходов!");
                        return result;
                    }
                    else
                    {
                        result.Add((Position.Item1, Position.Item2 + 1));
                        if (GameField[Position.Item1, Position.Item2 + 2] != null)
                        {
                            return result;
                        }
                        else
                        {
                            result.Add((Position.Item1, Position.Item2 + 2));
                            return result;
                        }
                    }
                }
                else
                {
                    if (GameField[Position.Item1, Position.Item2 - 1] != null)
                    {
                        Console.WriteLine("Нет свободных ходов!");
                        return result;
                    }
                    else
                    {
                        result.Add((Position.Item1, Position.Item2 - 1));
                        if (GameField[Position.Item1, Position.Item2 - 2] != null)
                        {
                            return result;
                        }
                        else
                        {
                            result.Add((Position.Item1, Position.Item2 - 2));
                            return result;
                        }
                    }
                }

            }
            else
            {

                if (Color == PieceColor.White)
                {
                    if (GameField[Position.Item1, Position.Item2 + 1] != null)
                    {
                        return result;
                    }
                    else
                    {
                        result.Add((Position.Item1, Position.Item2 + 1));
                        return result;
                    }
                }
                else
                {
                    if (GameField[Position.Item1, Position.Item2 - 1] != null)
                    {
                        return result;
                    }
                    else
                    {
                        result.Add((Position.Item1, Position.Item2 - 1));
                        return result;
                    }
                }

            }
        }

        public override string ToString()
        {
            return "p";
        }

        public List<(int, int)> AvalableKills(string[,] GameField)
        {
            var result = new List<(int, int)>();

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
                        if (GameField[Position.Item1 - 1, Position.Item2 + 1] != null && pieces.Contains(GameField[Position.Item1 - 1, Position.Item2 + 1]))
                        {
                            result.Add((Position.Item1 - 1, Position.Item2 + 1));
                        }
                }
                if (Position.Item1 < 7 && Position.Item2 < 7)
                {
                    if (Position.Item1 > 0 && Position.Item2 < 7)
                        if (GameField[Position.Item1 + 1, Position.Item2 + 1] != null && pieces.Contains(GameField[Position.Item1 + 1, Position.Item2 + 1]))
                        {
                            result.Add((Position.Item1 + 1, Position.Item2 + 1));
                        }
                }

                return result;
            }
            else
            {
                if (Position.Item1 > 0 && Position.Item2 > 0)
                {
                    if (GameField[Position.Item1 - 1, Position.Item2 - 1] != null && pieces.Contains(GameField[Position.Item1 - 1, Position.Item2 - 1]))
                    {
                        result.Add((Position.Item1 - 1, Position.Item2 - 1));
                    }
                }
                if (Position.Item1 < 7 && Position.Item2 > 0)
                {
                    if (GameField[Position.Item1 + 1, Position.Item2 - 1] != null && pieces.Contains(GameField[Position.Item1 + 1, Position.Item2 - 1]))
                    {
                        result.Add((Position.Item1 + 1, Position.Item2 - 1));
                    }
                }
                return result;
            }

        }

        public Pawn(PieceColor color, (int, int) position)
        {
            Color = color;
            startPos = position;
            Position = startPos;
            Dead = false;
        }
    }
}
