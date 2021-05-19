using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class King : IPiece
    {
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool IsDead { get; set; }

        public List<(int, int)> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<(int, int)>();
          
            if (Position.Item1 < 7 && Position.Item2 < 7)
            {
                if (GameField[Position.Item1 + 1, Position.Item2 + 1] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 + 1, Position.Item2 + 1));
                }

            }
            if (Position.Item2 < 7)
            {
                if (GameField[Position.Item1, Position.Item2 + 1] == " ")
                {
                    AvailableMovesList.Add((Position.Item1, Position.Item2 + 1));
                }

            }
            if (Position.Item1 > 0 && Position.Item2 < 7)
            {
                if (GameField[Position.Item1 - 1, Position.Item2 + 1] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 - 1, Position.Item2 + 1));
                }

            }
            if (Position.Item1 > 0)
            {
                if (GameField[Position.Item1 - 1, Position.Item2] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 - 1, Position.Item2));
                }

            }
            if (Position.Item1 < 7)
            {
                if (GameField[Position.Item1 + 1, Position.Item2] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 + 1, Position.Item2));
                }

            }

            if (Position.Item1 > 0 && Position.Item2 > 0)
            {
                if (GameField[Position.Item1 - 1, Position.Item2 - 1] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 - 1, Position.Item2 - 1));
                }

            }
            if (Position.Item2 > 0)
            {
                if (GameField[Position.Item1, Position.Item2 - 1] == " ")
                {
                    AvailableMovesList.Add((Position.Item1, Position.Item2 - 1));
                }

            }
            if (Position.Item1 < 7 && Position.Item2 > 0)
            {
                if (GameField[Position.Item1 + 1, Position.Item2 - 1] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 + 1, Position.Item2 - 1));
                }

            }

            return AvailableMovesList;

        }
        public King((int, int) Position, PieceColor color)
        {
            this.Position = Position;
            Color = color;
            IsDead = false;
        }
        public override string ToString()
        {
            return "k";
        }

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

            if (Position.Item1 < 7 && Position.Item2 < 7)
            {
                if (pieces.Contains(GameField[Position.Item1 + 1, Position.Item2 + 1]))
                {
                    AvailableKillsList.Add((Position.Item1 + 1, Position.Item2 + 1));
                }

            }
            if (Position.Item2 < 7)
            {
                if (pieces.Contains(GameField[Position.Item1, Position.Item2 + 1]))
                {
                    AvailableKillsList.Add((Position.Item1, Position.Item2 + 1));
                }

            }
            if (Position.Item1 > 0 && Position.Item2 < 7)
            {
                if (pieces.Contains(GameField[Position.Item1 - 1, Position.Item2 + 1]))
                {
                    AvailableKillsList.Add((Position.Item1 - 1, Position.Item2 + 1));
                }

            }
            if (Position.Item1 > 0)
            {
                if (pieces.Contains(GameField[Position.Item1 - 1, Position.Item2]))
                {
                    AvailableKillsList.Add((Position.Item1 - 1, Position.Item2));
                }

            }
            if (Position.Item1 < 7)
            {
                if (pieces.Contains(GameField[Position.Item1 + 1, Position.Item2]))
                {
                    AvailableKillsList.Add((Position.Item1 + 1, Position.Item2));
                }

            }

            if (Position.Item1 > 0 && Position.Item2 > 0)
            {
                if (pieces.Contains(GameField[Position.Item1 - 1, Position.Item2 - 1]))
                {
                    AvailableKillsList.Add((Position.Item1 - 1, Position.Item2 - 1));
                }

            }
            if (Position.Item2 > 0)
            {
                if (pieces.Contains(GameField[Position.Item1, Position.Item2 - 1]))
                {
                    AvailableKillsList.Add((Position.Item1, Position.Item2 - 1));
                }

            }
            if (Position.Item1 < 7 && Position.Item2 > 0)
            {
                if (pieces.Contains(GameField[Position.Item1 + 1, Position.Item2 - 1]))
                {
                    AvailableKillsList.Add((Position.Item1 + 1, Position.Item2 - 1));
                }

            }

            return AvailableKillsList;
        }
    }
}
