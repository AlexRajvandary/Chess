using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;

namespace ChessLib
{
    public interface IBoardState
    {
        IPieceInfo GetPieceAt(Position position);
        bool IsCellFree(Position position);
        IReadOnlyList<Position> GetAttackedCells(PieceColor color);
    }
}