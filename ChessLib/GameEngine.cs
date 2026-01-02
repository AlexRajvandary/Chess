using ChessLib.Common;
using ChessLib.Pieces;
using ChessLib.Services;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib
{
    public class GameEngine : IGameEngine
    {
        private readonly Game game;

        public GameEngine()
        {
            game = new Game();
        }

        public MoveResult MakeMove(Position from, Position to)
        {
            return game.MakeMove(from, to);
        }

        public IReadOnlyList<Position> GetValidMoves(Position position)
        {
            return game.GetValidMoves(position).AsReadOnly();
        }

        public IGameState GetState()
        {
            return game.GetState();
        }

        public string GetFen()
        {
            return game.GetFen();
        }

        public string GetMoveHistory()
        {
            return game.GetMoveHistory();
        }

        public void StartNewGame()
        {
            game.StartNewGame();
        }

        public void EndGameByTime(PieceColor losingColor)
        {
            game.EndGameByTime(losingColor);
        }

        public void RestoreFromFen(string fen)
        {
            game.RestoreFromFen(fen);
        }

        public void RestoreFromMoveHistory(IEnumerable<string> moves)
        {
            game.RestoreFromMoveHistory(moves);
        }

        public void RestoreFromPgn(string pgn)
        {
            game.RestoreFromPgn(pgn);
        }
    }
}

