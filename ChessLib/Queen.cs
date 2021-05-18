using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class Queen : IPiece
    {
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool Dead { get; set; }

        public List<(int, int)> AvalableMoves(string[,] GameField)
        {
            var result = new List<(int, int)>();
            //Северо-восток
            for (int i = Position.Item1, j = Position.Item2; i < 8 & j < 8; i++, j++)
            {
                if (GameField[i, j] != " ")
                {
                    break;
                }
                result.Add((i, j));
            }
            //северо-запад
            for (int i = Position.Item1, j = Position.Item2; i > -1 & j < 8; i--, j++)
            {
                if (GameField[i, j] != " ")
                {
                    break;
                }
                result.Add((i, j));
            }
            //Юго-Восток
            for (int i = Position.Item1, j = Position.Item2; i < 8 & j > -1; i++, j--)
            {
                if (GameField[i, j] != " ")
                {
                    break;
                }
                result.Add((i, j));
            }
            //Юго-Запад
            for (int i = Position.Item1, j = Position.Item2; i > -1 & j > -1; i--, j--)
            {
                if (GameField[i, j] != " ")
                {
                    break;
                }
                result.Add((i, j));
            }
            //Север
            for (int j = Position.Item2; j < 8; j++)
            {
                if (GameField[Position.Item1, j] != " ")
                {
                    break;
                }
                result.Add((Position.Item1, j));
            }
            //Юг
            for (int j = Position.Item2; j > -1; j--)
            {
                if (GameField[Position.Item1, j] != " ")
                {
                    break;
                }
                result.Add((Position.Item1, j));
            }
            //Запад
            for (int i = Position.Item1; i < 8; i++)
            {
                if (GameField[i, Position.Item2] != " ")
                {
                    break;
                }
                result.Add((i, Position.Item2));
            }
            //восток
            for (int i = Position.Item1; i > -1; i--)
            {
                if (GameField[i, Position.Item2] != " ")
                {
                    break;
                }
                result.Add((i, Position.Item2));
            }
            return result;
        }
        public override string ToString()
        {
            return "q";
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
            //Север
            for (int j = Position.Item2; j < 8; j++)
            {

                if (pieces.Contains(GameField[Position.Item1, j]))
                {
                    result.Add((Position.Item1, j));
                    break;
                }
            }
            //Юг
            for (int j = Position.Item2; j > -1; j--)
            {
                if (pieces.Contains(GameField[Position.Item1, j]))
                {
                    result.Add((Position.Item1, j));
                    break;
                }

            }
            //Запад
            for (int i = Position.Item1; i < 8; i++)
            {
                if (pieces.Contains(GameField[i, Position.Item2]))
                {
                    result.Add((i, Position.Item2));
                    break;
                }

            }
            //восток
            for (int i = Position.Item1; i > -1; i--)
            {
                if (pieces.Contains(GameField[i, Position.Item2]))
                {
                    result.Add((i, Position.Item2));
                    break;
                }

            }
            //Северо-восток
            for (int i = Position.Item1 + 1, j = Position.Item2 + 1; i < 8 & j < 8; i++, j++)
            {
                if (pieces.Contains(GameField[i, j]))
                {
                    result.Add((i, j));
                    break;
                }

            }
            //северо-запад
            for (int i = Position.Item1 - 1, j = Position.Item2 + 1; i > -1 & j < 8; i--, j++)
            {
                if (pieces.Contains(GameField[i, j]))
                {
                    result.Add((i, j));
                    break;
                }

            }
            //Юго-Восток
            for (int i = Position.Item1 + 1, j = Position.Item2 - 1; i < 8 & j > -1; i++, j--)
            {
                if (pieces.Contains(GameField[i, j]))
                {
                    result.Add((i, j));
                    break;
                }
            }
            //Юго-Запад
            for (int i = Position.Item1 - 1, j = Position.Item2 - 1; i > -1 & j > -1; i--, j--)
            {
                if (pieces.Contains(GameField[i, j]))
                {
                    result.Add((i, j));
                    break;
                }
            }
            return result;
        }

        public Queen(PieceColor color, (int,int) startPos)
        {
            Position = startPos;
            Color = color;
            Dead = false;
        }
    }
}
