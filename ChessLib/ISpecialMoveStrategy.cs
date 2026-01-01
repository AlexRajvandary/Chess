using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;

namespace ChessLib
{
    public interface ISpecialMoveStrategy
    {
        List<Position> GetPossibleSpecialMoves(IPiece piece, List<IPiece> allPieces, IBoardQuery board);
        bool CanExecuteSpecialMove(IPiece piece, Position destination, List<IPiece> allPieces, IBoardQuery board);
    }
}

