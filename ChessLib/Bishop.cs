using System.Collections.Generic;

namespace ChessLib
{
    public class Bishop : IPiece
    {
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool Dead { get; set; }

        /// <summary>
        /// Проверка доступных ходов для слона в четырех направлениях
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns>Список координат свободных для хода клеток</returns>
        public List<(int, int)> AvalableMoves(string[,] GameField)
        {
            var result = new List<(int, int)>();
            //Северо-восток
            for(int i = Position.Item1+1, j = Position.Item2+1; i<8 & j < 8;i++,j++)
            {
                if(GameField[i,j] != " ")
                {
                    break;
                }
                result.Add((i, j));
            }
            //северо-запад
            for (int i = Position.Item1-1, j = Position.Item2+1; i > -1 & j < 8; i--, j++)
            {
                if (GameField[i, j] != " ")
                {
                    break;
                }
                result.Add((i, j));
            }
            //Юго-Восток
            for (int i = Position.Item1+1, j = Position.Item2-1; i < 8 & j > -1; i++, j--)
            {
                if (GameField[i, j] != " ")
                {
                    break;
                }
                result.Add((i, j));
            }
            //Юго-Запад
            for (int i = Position.Item1-1, j = Position.Item2-1; i > -1 & j > -1; i--, j--)
            {
                if (GameField[i, j] != " ")
                {
                    break;
                }
                result.Add((i, j));
            }
            return result;
        }
        public override string ToString()
        {
            return "b";
        }
      
        public List<(int, int)> AvalableKills(string[,] GameField)
        {
            var result = new List<(int, int)>();
            string pieces;
            if(Color == PieceColor.White)
            {
                pieces = "bnpqr";
            }
            else
            {
                pieces = "BNPQR";
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

        public Bishop((int,int)startPosition, PieceColor Color)
        {
            this.Color = Color;
            Position = startPosition;
            Dead = false;
        }
    }
}
