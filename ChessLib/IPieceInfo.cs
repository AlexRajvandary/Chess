using ChessLib.Common;
using ChessLib.Pieces;

namespace ChessLib
{
    public interface IPieceInfo
    {
        PieceType Type { get; }
        PieceColor Color { get; }
        Position Position { get; }
    }
}