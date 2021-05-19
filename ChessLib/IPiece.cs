using System.Collections.Generic;

namespace ChessLib
{
    /// <summary>
    /// Цвет фигур
    /// </summary>
    public enum PieceColor
    {
        White,
        Black
    }
    public interface IPiece
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
        /// Позиция на доске
        /// </summary>
        public (int, int) Position { get; set; }
        /// <summary>
        /// Доступные клетки для хода
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns></returns>
        public List<(int,int)> AvailableMoves(string[,] GameField);
        /// <summary>
        /// Проверяет какие фигуры можно съесть
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public List<(int, int)> AvailableKills(string[,] GameField);
    
    }
}
