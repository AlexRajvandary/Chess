namespace ChessBoard
{
    public class Cell : NotifyPropertyChanged
    {
        private State _state;
       
        private bool _active;

        public Position Position { get; }
        public State State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }
     
        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                OnPropertyChanged();
            }
        }
        public Cell(int Vertical,int Horizontal)
        {
            Position = new Position(Vertical,Horizontal);
        }
    }
    public class Position
    {
        /// <summary>
        /// Позиция по вертикали
        /// </summary>
        public int Horizontal { get; }
        /// <summary>
        /// Позиция по горизонтали
        /// </summary>
        public int Vertical { get; }

        public Position(int Vertical, int Horizontal)
        {
            this.Horizontal = Horizontal;
            this.Vertical = Vertical;
        }
    }
}