using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChessBoard
{
    public class Board : IEnumerable<Cell>
    {
        private readonly Cell[,] _area;

        public State this[int row, int column]
        {
            get => _area[row, column].State;
            set => _area[row, column].State = value;
        }
       

        public Board()
        {
            _area = new Cell[8, 8];
            for (int i = 0; i < _area.GetLength(0); i++)
                for (int j = 0; j < _area.GetLength(1); j++)
                    _area[i, j] = new Cell(_area.GetLength(0) - i - 1, j );// Для удобства, чтобы клетка в 0 столбце, 0 строке была именно (0,0), а не (1,0)

            
        }

        public IEnumerator<Cell> GetEnumerator() 
            => _area.Cast<Cell>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() 
            => _area.GetEnumerator();
    }
}