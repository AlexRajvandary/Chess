using ChessLib.Pieces;
using System.Collections.Generic;

namespace ChessLib.Services
{
    public class GameState
    {
        public PieceColor CurrentPlayerColor { get; set; }
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public bool IsStalemate { get; set; }
        public bool IsGameOver { get; set; }
        public string[,] BoardRepresentation { get; set; }
        public List<IPiece> Pieces { get; set; }
        public string GameOverReason { get; set; }

        public GameState()
        {
            Pieces = new List<IPiece>();
            BoardRepresentation = new string[8, 8];
        }
    }
}