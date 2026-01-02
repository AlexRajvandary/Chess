using ChessLib.Pieces;
using System.Collections.Generic;

namespace ChessLib
{
    public interface IGameState
    {
        PieceColor CurrentPlayerColor { get; }
        bool IsCheck { get; }
        bool IsCheckmate { get; }
        bool IsGameOver { get; }
        IReadOnlyList<IPieceInfo> Pieces { get; }
    }
}