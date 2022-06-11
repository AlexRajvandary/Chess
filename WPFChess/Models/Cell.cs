namespace ChessBoard
{
    /// <summary>
    /// Клетка
    /// </summary>
    public class Cell : NotifyPropertyChanged
    {
        private State _state;
        private bool _active;

        public Cell(int horizontal, int vertical)
        {
            Position = new Position(horizontal, vertical);
        }

        /// <summary>
        /// Позиция клетки на игровой доске описывается двумя натуральными числами (по горизонтали, по вертикали)
        /// </summary>
        public Position Position { get; set; }
        /// <summary>
        /// Клетка может иметь одно из 13 состояний
        /// <para>"Пустая клетка","Белая пешка","Черный Ферзь" и т.д.
        /// </para>
        /// </summary>
        public State State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Клетка активна, если на нее нажал пользователь
        /// </summary>
        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                OnPropertyChanged();
            }
        }
    }
}