using ChessWPF.Models;

namespace ChessWPF.ViewModels
{
    public class CellViewModel : NotifyPropertyChanged
    {
        private bool _active;
        private CellUIState _state;
       
        public CellViewModel(int horizontal, int vertical)
        {
            Position = new Position(horizontal, vertical);
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

        public Position Position { get; set; }
      
        public CellUIState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }
    }
}