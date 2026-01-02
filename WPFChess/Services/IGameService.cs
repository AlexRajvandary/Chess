using ChessLib;
using ChessLib.Common;
using System.Collections.Generic;

namespace ChessWPF.Services
{
    public interface IGameService
    {
        MoveResult MakeMove(ChessLib.Common.Position from, ChessLib.Common.Position to);
        IGameState GetState();
        IReadOnlyList<ChessLib.Common.Position> GetValidMoves(ChessLib.Common.Position position);
        string GetFen();
        string GetMoveHistory();
        void StartNewGame();
        void EndGameByTime(ChessLib.Pieces.PieceColor losingColor);
    }
}