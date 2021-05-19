using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class Knight : IPiece
    {
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool IsDead { get; set; }

        public List<(int, int)> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<(int, int)>();

            if (Position.Item1 < 7 && Position.Item2 < 6)
            {
                if (GameField[Position.Item1 + 1, Position.Item2 + 2]==" ")
                {
                    AvailableMovesList.Add((Position.Item1 + 1, Position.Item2 + 2));
                }

            }
            if (Position.Item1 < 6 && Position.Item2 < 7)
            {
                if (GameField[Position.Item1 + 2, Position.Item2 + 1]==" ")
                {
                    AvailableMovesList.Add((Position.Item1 + 2, Position.Item2 + 1));
                }

            }
            if (Position.Item1 < 6 && Position.Item2 > 0)
            {
                if (GameField[Position.Item1 + 2, Position.Item2 - 1]==" ")
                {
                    AvailableMovesList.Add((Position.Item1 + 2, Position.Item2 - 1));
                }

            }
            if (Position.Item1 < 7 && Position.Item2 > 1)
            {
                if (GameField[Position.Item1 + 1, Position.Item2 - 2]==" ")
                {
                    AvailableMovesList.Add((Position.Item1 + 1, Position.Item2 - 2));
                }

            }
            if (Position.Item1 > 0 && Position.Item2 < 6)
            {
                if (GameField[Position.Item1 - 1, Position.Item2 + 2]==" ")
                {
                    AvailableMovesList.Add((Position.Item1 - 1, Position.Item2 + 2));
                }

            }
            if (Position.Item1 > 1 && Position.Item2 < 7)
            {
                if (GameField[Position.Item1 - 2, Position.Item2 + 1] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 - 2, Position.Item2 + 1));
                }

            }
            if (Position.Item1 > 1 && Position.Item2 > 0)
            {
                if (GameField[Position.Item1 - 2, Position.Item2 - 1] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 - 2, Position.Item2 - 1));
                }

            }
            if (Position.Item1 > 0 && Position.Item2 > 1)
            {
                if (GameField[Position.Item1 - 1, Position.Item2 - 2] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 - 1, Position.Item2 - 2));
                }

            }

            return AvailableMovesList;
        }
        public Knight((int,int) startPos, PieceColor color)
        {
            Position = startPos;
            Color = color;
            IsDead = false;
        }
        public override string ToString()
        {
            return "n";
        }
        private string pieces;
        public List<(int, int)> AvailableKills(string[,] GameField)
        {
            var result = new List<(int, int)>();
           
            if (Color == PieceColor.White)
            {
                pieces = "bnpqr";
            }
            else
            {
                pieces = "BNPQR";
            }
           
                if (Position.Item1 < 7 && Position.Item2 < 6)
                {
                    if (GameField[Position.Item1 + 1, Position.Item2 + 2] != null && pieces.Contains(GameField[Position.Item1 + 1, Position.Item2 + 2]))
                    {
                        result.Add((Position.Item1 + 1, Position.Item2 + 2));
                    }

                }
                if (Position.Item1 < 6 && Position.Item2 < 7)
                {
                    if (GameField[Position.Item1 + 2, Position.Item2 + 1] != null && pieces.Contains(GameField[Position.Item1 + 2, Position.Item2 + 1]))
                    {
                        result.Add((Position.Item1 + 2, Position.Item2 + 1));
                    }

                }
                if (Position.Item1 < 6 && Position.Item2 > 0)
                {
                    if (GameField[Position.Item1 + 2, Position.Item2 - 1] != null && pieces.Contains(GameField[Position.Item1 + 2, Position.Item2 - 1]))
                    {
                        result.Add((Position.Item1 + 2, Position.Item2 - 1));
                    }

                }
                if (Position.Item1 < 7 && Position.Item2 > 1)
                {
                    if (GameField[Position.Item1 + 1, Position.Item2 - 2]!= null && pieces.Contains(GameField[Position.Item1 + 1, Position.Item2 - 2]))
                    {
                        result.Add((Position.Item1 + 1, Position.Item2 - 2));
                    }

                }
                if (Position.Item1 > 0 && Position.Item2 < 6)
                {
                    if (GameField[Position.Item1 - 1, Position.Item2 + 2] != null && pieces.Contains(GameField[Position.Item1 - 1, Position.Item2 + 2]))
                    {
                        result.Add((Position.Item1 - 1, Position.Item2 + 2));
                    }

                }
                if (Position.Item1 > 1 && Position.Item2 < 7)
                {
                    if (GameField[Position.Item1 - 2, Position.Item2 + 1] != null && pieces.Contains(GameField[Position.Item1 - 2, Position.Item2 + 1]))
                    {
                        result.Add((Position.Item1 - 2, Position.Item2 + 1));
                    }

                }
                if (Position.Item1 > 1 && Position.Item2 > 0)
                {
                    if (GameField[Position.Item1 - 2, Position.Item2 - 1] != null && pieces.Contains(GameField[Position.Item1 - 2, Position.Item2 - 1]))
                    {
                        result.Add((Position.Item1 - 2, Position.Item2 - 1));
                    }

                }
                if (Position.Item1 > 0 && Position.Item2 > 1)
                {
                    if (GameField[Position.Item1 - 1, Position.Item2 - 2] != null && pieces.Contains(GameField[Position.Item1 - 1, Position.Item2 - 2]))
                    {
                        result.Add((Position.Item1 - 1, Position.Item2 - 2));
                    }

                }


            
          

            return result;
        }
    }
}
