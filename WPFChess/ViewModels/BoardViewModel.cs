using ChessWPF.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChessWPF.ViewModels
{
    public class BoardViewModel : IEnumerable<CellViewModel>
    {
        private readonly CellViewModel[,] _area;
        
        public CellUIState this[int row, int column]
        {
            get => _area[row, column].State;
            set => _area[row, column].State = value;
        }

        public BoardViewModel()
        {
            _area = new CellViewModel[8, 8];
            for (int vertical = 0; vertical < _area.GetLength(0); vertical++)
                for (int horizontal = 0; horizontal < _area.GetLength(1); horizontal++)
                    _area[vertical, horizontal] = new CellViewModel(horizontal, _area.GetLength(0) - vertical - 1);
        }

        public IEnumerator<CellViewModel> GetEnumerator()
            => _area.Cast<CellViewModel>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _area.GetEnumerator();
    }
}