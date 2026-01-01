using ChessLib.Common;
using ChessLib.Pieces;

namespace ChessLib
{
    public interface IBoardQuery
    {
        IPieceInfo GetPieceAt(Position position);
        bool IsCellFree(Position position);
        bool IsCellAttacked(Position position, PieceColor byColor);
    }
}

