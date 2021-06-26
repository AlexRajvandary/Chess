using System.Text;

namespace ChessLib
{
    public static class Fen
    {
        public static GameField GameField { get; set; }

        public static int CurrentPlayer { get; set; }

        public static string GetFenFromTheGameField()
        {
            StringBuilder fenStr = new StringBuilder();

            for (int row = 0; row < 8; row++)
            {

                int numberOfEmptyCells = 0;

                for (int col = 0; col < 8; col++)
                {
                    if (GameField[col, row].isFilled)
                    {
                        numberOfEmptyCells = 0;

                        fenStr.Append(GameField[col, row].Piece);
                    }
                    else
                    {
                        numberOfEmptyCells++;

                        if (col == 7 || GameField[col + 1, row].isFilled)
                        {
                            fenStr.Append($"{numberOfEmptyCells}");
                        }

                    }

                }
                NextLine(fenStr, row);

            }
            AddCurrentPlayerStatus(fenStr);

            return fenStr.ToString();
        }

        private static void NextLine(StringBuilder fenStr, int i)
        {
            if (i < 7)
            {
                fenStr.Append("/");
            }
        }

        private static void AddCurrentPlayerStatus(StringBuilder fenStr)
        {
            if (CurrentPlayer == 0)
            {
                fenStr.Append(" w");
            }
            else
            {
                fenStr.Append(" b");
            }
        }
    }
}
