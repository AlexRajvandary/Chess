using ChessWPF.Models;
using System.Windows;
using System.Windows.Controls;

namespace ChessWPF.Controls
{
    /// <summary>
    /// Interaction logic for ChessPiece.xaml
    /// </summary>
    public partial class ChessPiece : UserControl
    {
        public static readonly DependencyProperty PieceProperty = DependencyProperty.Register("Piece", typeof(CellUIState), typeof(ChessPiece));

        public CellUIState Piece
        {
            get => (CellUIState)GetValue(PieceProperty);
            set => SetValue(PieceProperty, value);
        }

        public ChessPiece()
        {
            InitializeComponent();
        }
    }
}
