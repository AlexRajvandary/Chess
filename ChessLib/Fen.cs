using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLib
{
    public static class Fen
    {
        public static GameField GameField { get; set; }

        public static int CurrentPlayer { get; set; }
      
        public static string GetFenFromTheGameField()
        {
            StringBuilder fenStr = new StringBuilder();

            for (int i = 0; i < 8; i++)
            {

                int numberOfEmptyCells = 0;

                for (int j = 0; j < 8; j++)
                {
                    if (GameField[j,i].isFilled)
                    {
                        numberOfEmptyCells = 0;

                        fenStr.Append(GameField[j,i].Piece);
                    }
                    else
                    {
                        numberOfEmptyCells++;

                        if(j == 7 || GameField[j + 1,i].isFilled)
                        {
                            fenStr.Append($"{numberOfEmptyCells}");
                        }
                       
                    }
                   
                }
              fenStr.Append("/");
            }
            if(CurrentPlayer == 0)
            {
                fenStr.Append(" w");
            }
            else
            {
                fenStr.Append(" b");
            }
          

            return fenStr.ToString();
        }
    }
}
