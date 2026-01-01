using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;

namespace ChessLib
{
    public interface IGameEngine
    {
        MoveResult MakeMove(Position from, Position to);
        IReadOnlyList<Position> GetValidMoves(Position position);
        IGameState GetState();
        string GetFen();
        string GetMoveHistory();
        void StartNewGame();
        void EndGameByTime(PieceColor losingColor);
    }
}
