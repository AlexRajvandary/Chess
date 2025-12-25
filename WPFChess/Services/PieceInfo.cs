using ChessLib.Pieces;

namespace ChessWPF.Services
{
    /// <summary>
    /// Information about a piece on the board
    /// </summary>
    public class PieceInfo
    {
        public IPiece Piece { get; set; }
        public PieceColor Color { get; set; }
        public PieceType Type { get; set; }
    }
}

