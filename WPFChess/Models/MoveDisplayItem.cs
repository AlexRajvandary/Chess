using System.Windows.Input;

namespace ChessWPF.Models
{
    /// <summary>
    /// Represents a move in the move history for display and selection
    /// </summary>
    public class MoveDisplayItem
    {
        public int MoveNumber { get; set; }
        public int WhiteMoveIndex { get; set; }
        public int BlackMoveIndex { get; set; }
        public string WhiteMoveText { get; set; }
        public string BlackMoveText { get; set; }
        public bool IsWhiteSelected { get; set; }
        public bool IsBlackSelected { get; set; }
        public ICommand SelectWhiteMoveCommand { get; set; }
        public ICommand SelectBlackMoveCommand { get; set; }
    }
}
