using ChessLib.Pieces;

namespace ChessLib.Common
{
    public class Cell
    {
        private IPiece piece;

        public Cell()
        {
            IsAtacked = false;
            piece = null;
        }

        public bool IsAtacked { get; set; }

        /// <summary>
        /// Returns true if cell contains a piece
        /// </summary>
        public bool IsFilled => piece != null;

        public IPiece Piece
        {
            get => piece;
            set
            {
                piece = value;
            }
        }
    }
}