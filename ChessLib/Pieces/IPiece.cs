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
        /// Available cells for move
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns></returns>
        public List<Position> AvailableMoves(string[,] GameField);
        /// <summary>
        /// Changes piece position
        /// </summary>
        /// <param name="position">New position</param>
        public void ChangePosition(Position position);
        /// <summary>
        /// Checks which pieces can be captured
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns></returns>
        public List<Position> AvailableKills(string[,] GameField);
    }
}