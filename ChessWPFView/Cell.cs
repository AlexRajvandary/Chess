using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPFView
{
  
    public class Cell : NotifyPropertyChanged
    {
        private State _state;
        private bool _active;

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
    }

}
