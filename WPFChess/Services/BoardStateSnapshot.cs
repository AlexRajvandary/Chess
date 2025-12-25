using ChessLib.Pieces;

namespace ChessWPF.Services
{
    /// <summary>
    /// Snapshot of board state for UI consumption
    /// </summary>
    public class BoardStateSnapshot
    {
        public PieceInfo[,] Cells { get; set; } = new PieceInfo[8, 8];
        public PieceColor CurrentPlayerColor { get; set; }
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public bool IsGameOver { get; set; }
    }
}

