using System.Collections.Generic;

namespace ChessLib
{
    public class Rook : IPiece
    {
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool Dead { get; set; }

        /// <summary>
        /// Проверяет доступные для ладьи ходы в 4 направлениях
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns></returns>
        public List<(int, int)> AvalableMoves(string[,] GameField)
        {
            var result = new List<(int, int)>();
            //Север
            for (int j = Position.Item2; j < 8; j++)
            {
                if (GameField[Position.Item1, j] != null)
                {
                    break;
                }
                result.Add((Position.Item1, j));
            }
            //Юг
            for (int j = Position.Item2; j > -1; j--)
            {
                if (GameField[Position.Item1, j] != null)
                {
                    break;
                }
                result.Add((Position.Item1, j));
            }
            //Запад
            for (int i = Position.Item1; i < 8; i++)
            {
                if (GameField[i, Position.Item2] != null)
                {
                    break;
                }
                result.Add((i, Position.Item2));
            }
            //восток
            for (int i = Position.Item1; i > -1; i--)
            {
                if (GameField[i, Position.Item2] != null)
                {
                    break;
                }
                result.Add((i, Position.Item2));
            }
            return result;
        }
        public override string ToString()
        {
            return "r";
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
            return result;
        }

        public Rook((int,int)Position,PieceColor color)
        {
            this.Position = Position;
            Color = color;
            Dead = false;
        }
    }
}
