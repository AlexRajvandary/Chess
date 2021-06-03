namespace ChessBoard
{
    public class Cell : NotifyPropertyChanged
    {
        private State _state;
        private bool _active;
        public Position Position { get; set; }
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
        public Cell(int horizontal, int vertical)
        {
            Position = new Position(horizontal, vertical);
        }
    }
    public class Position
    {
        public int Vertical { get; set; }

        public int Horizontal { get; set; }

        public override string ToString()
        {
            return $"{"ABCDEFGH"[this.Horizontal]}{this.Vertical + 1}";
        }
        public Position(int Horizontal, int Vertical)
        {
            this.Horizontal = Horizontal;
            this.Vertical = Vertical;
        }
    }
}