using ChessLib.Common;
using System;
using System.Collections.Generic;

namespace ChessLib.Pieces
{
    public interface IPiece : ICloneable
    {
        /// <summary>
        /// Является ли фигура убитой
        /// </summary>
        public bool IsDead { get; set; }
        /// <summary>
        /// Цвет фигуры
        /// </summary>
        public PieceColor Color { get; set; }
        /// <summary>
        /// Position on the board
        /// </summary>
        public Position Position { get; set; }
        /// <summary>
        /// Changes piece position
        /// </summary>
        /// <param name="position">New position</param>
        public void ChangePosition(Position position);
    }
}