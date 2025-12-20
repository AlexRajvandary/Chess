using System.ComponentModel;

namespace ChessWPF.Models
{
    /// <summary>
    /// Represents a captured piece with its count
    /// </summary>
    public class CapturedPieceInfo : INotifyPropertyChanged
    {
        private int count;

        public CellUIState Piece { get; set; }
        
        public int Count
        {
            get => count;
            set
            {
                count = value;
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        public string DisplayText => Count > 1 ? Count.ToString() : string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
