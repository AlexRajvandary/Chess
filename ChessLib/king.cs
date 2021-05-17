using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class king : IPiece
    {
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool Dead { get; set; }

        public List<(int, int)> AvalableMoves(string[,] GameField)
        {
            var result = new List<(int, int)>();
          
            if (Position.Item1 < 7 && Position.Item2 < 7)
            {
                if (GameField[Position.Item1 + 1, Position.Item2 + 1] == null)
                {
                    result.Add((Position.Item1 + 1, Position.Item2 + 1));
                }

            }
            if (Position.Item2 < 7)
            {
                if (GameField[Position.Item1, Position.Item2 + 1] == null)
                {
                    result.Add((Position.Item1, Position.Item2 + 1));
                }

            }
            if (Position.Item1 > 0 && Position.Item2 < 7)
            {
                if (GameField[Position.Item1 - 1, Position.Item2 + 1] == null)
                {
                    result.Add((Position.Item1 - 1, Position.Item2 + 1));
                }

            }
            if (Position.Item1 > 0)
            {
                if (GameField[Position.Item1 - 1, Position.Item2] == null)
                {
                    result.Add((Position.Item1 - 1, Position.Item2));
                }

            }
            if (Position.Item1 < 7)
            {
                if (GameField[Position.Item1 + 1, Position.Item2] == null)
                {
                    result.Add((Position.Item1 + 1, Position.Item2));
                }

            }

            if (Position.Item1 > 0 && Position.Item2 > 0)
            {
                if (GameField[Position.Item1 - 1, Position.Item2 - 1] == null)
                {
                    result.Add((Position.Item1 - 1, Position.Item2 - 1));
                }

            }
            if (Position.Item2 > 0)
            {
                if (GameField[Position.Item1, Position.Item2 - 1] == null)
                {
                    result.Add((Position.Item1, Position.Item2 - 1));
                }

            }
            if (Position.Item1 < 7 && Position.Item2 > 0)
            {
                if (GameField[Position.Item1 + 1, Position.Item2 - 1] == null)
                {
                    result.Add((Position.Item1 + 1, Position.Item2 - 1));
                }

            }

            return result;

        }
        public king((int, int) Position, PieceColor color)
        {
            this.Position = Position;
            Color = color;
            Dead = false;
        }
        public override string ToString()
        {
            return "k";
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

            if (Position.Item1 < 7 && Position.Item2 < 7)
            {
                if (pieces.Contains(GameField[Position.Item1 + 1, Position.Item2 + 1]))
                {
                    result.Add((Position.Item1 + 1, Position.Item2 + 1));
                }

            }
            if (Position.Item2 < 7)
            {
                if (pieces.Contains(GameField[Position.Item1, Position.Item2 + 1]))
                {
                    result.Add((Position.Item1, Position.Item2 + 1));
                }

            }
            if (Position.Item1 > 0 && Position.Item2 < 7)
            {
                if (pieces.Contains(GameField[Position.Item1 - 1, Position.Item2 + 1]))
                {
                    result.Add((Position.Item1 - 1, Position.Item2 + 1));
                }

            }
            if (Position.Item1 > 0)
            {
                if (pieces.Contains(GameField[Position.Item1 - 1, Position.Item2]))
                {
                    result.Add((Position.Item1 - 1, Position.Item2));
                }

            }
            if (Position.Item1 < 7)
            {
                if (pieces.Contains(GameField[Position.Item1 + 1, Position.Item2]))
                {
                    result.Add((Position.Item1 + 1, Position.Item2));
                }

            }

            if (Position.Item1 > 0 && Position.Item2 > 0)
            {
                if (pieces.Contains(GameField[Position.Item1 - 1, Position.Item2 - 1]))
                {
                    result.Add((Position.Item1 - 1, Position.Item2 - 1));
                }

            }
            if (Position.Item2 > 0)
            {
                if (pieces.Contains(GameField[Position.Item1, Position.Item2 - 1]))
                {
                    result.Add((Position.Item1, Position.Item2 - 1));
                }

            }
            if (Position.Item1 < 7 && Position.Item2 > 0)
            {
                if (pieces.Contains(GameField[Position.Item1 + 1, Position.Item2 - 1]))
                {
                    result.Add((Position.Item1 + 1, Position.Item2 - 1));
                }

            }

            return result;
        }
    }
}
