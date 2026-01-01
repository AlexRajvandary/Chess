using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;

namespace ChessLib.Caching
{
    public interface IMoveCache
    {
        List<Position> GetMoves(PieceType pieceType, PieceColor color, Position position, string boardState);
        List<Position> GetCaptures(PieceType pieceType, PieceColor color, Position position, string boardState);
        void SetMoves(PieceType pieceType, PieceColor color, Position position, string boardState, List<Position> moves);
        void SetCaptures(PieceType pieceType, PieceColor color, Position position, string boardState, List<Position> captures);
        void Clear();
    }
}

