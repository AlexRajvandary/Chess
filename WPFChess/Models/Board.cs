﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChessBoard
{
    /// <summary>
    /// Игровое поле
    /// </summary>
    public class Board : IEnumerable<Cell>
    {
        private readonly Cell[,] _area;
        /// <summary>
        /// Доступ к каждой клетке игрового поля по индексу
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public State this[int row, int column]
        {
            get => _area[row, column].State;
            set => _area[row, column].State = value;
        }

        public Board()
        {
            _area = new Cell[8, 8];
            for (int vertical = 0; vertical < _area.GetLength(0); vertical++)
                for (int horizontal = 0; horizontal < _area.GetLength(1); horizontal++)
                    _area[vertical, horizontal] = new Cell(horizontal, _area.GetLength(0) - vertical - 1);
        }

        public IEnumerator<Cell> GetEnumerator()
            => _area.Cast<Cell>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _area.GetEnumerator();
    }
}