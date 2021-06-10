using System.Windows;
using System.Windows.Controls;

namespace ChessBoard.Controls
{
    /// <summary>
    /// Interaction logic for ChessPiece.xaml
    /// </summary>
    public partial class ChessPiece : UserControl
    {
        public static readonly DependencyProperty PieceProperty = DependencyProperty.Register("Piece", typeof(State), typeof(ChessPiece));

        public State Piece
        {
            get => (State)GetValue(PieceProperty);
            set => SetValue(PieceProperty, value);
        }

        public ChessPiece()
        {
            InitializeComponent();
        }
    }
}
