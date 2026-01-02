using ChessLib.Common;
using System.Collections.Generic;

namespace ChessLib
{
    public interface IMoveStrategy
    {
        PieceType PieceType { get; }
        List<Position> GetPossibleMoves(IPieceInfo piece, IBoardQuery board);
        List<Position> GetPossibleCaptures(IPieceInfo piece, IBoardQuery board);
    }
}

