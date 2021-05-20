using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLib
{
    public interface IView
    {
        /// <summary>
        /// Визуализирует данные
        /// </summary>
        /// <param name="msg">Передоваемая информация</param>
        public void Show(string msg);

        public void Visualize(string[,] GameField, int CurrentPlayer);
    }
}
