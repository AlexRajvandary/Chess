namespace ChessWPF
{
    /// <summary>
    /// Позиция 
    /// </summary>
    public class Position
    {
        /// <summary>
        /// Позиция по вертикали
        /// </summary>
        public int Vertical { get; set; }
        /// <summary>
        /// Позиция по горизонтали
        /// </summary>
        public int Horizontal { get; set; }
        /// <summary>
        /// Текстовое представление шахматной позиции
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{"ABCDEFGH"[Horizontal]}{Vertical + 1}";
        }
        public Position(int Horizontal, int Vertical)
        {
            this.Horizontal = Horizontal;
            this.Vertical = Vertical;
        }
    }
}