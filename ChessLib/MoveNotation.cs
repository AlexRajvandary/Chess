namespace ChessLib
{
    /// <summary>
    /// Represents a move in algebraic notation
    /// </summary>
    public class MoveNotation
    {
        public Position From { get; set; }
        public Position To { get; set; }
        public IPiece Piece { get; set; }
        public MoveType MoveType { get; set; }
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public IPiece CapturedPiece { get; set; }
        public int MoveNumber { get; set; }
        public PieceColor PlayerColor { get; set; }
    }
}
