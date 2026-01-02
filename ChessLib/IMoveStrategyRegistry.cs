using ChessLib.Common;

namespace ChessLib
{
    public interface IMoveStrategyRegistry
    {
        IMoveStrategy GetStrategy(PieceType pieceType);
    }
}

