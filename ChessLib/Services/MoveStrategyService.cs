using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib.Services
{
    public class MoveStrategyService
    {
        private readonly IMoveStrategyRegistry strategyRegistry;

        public MoveStrategyService(IMoveStrategyRegistry strategyRegistry)
        {
            this.strategyRegistry = strategyRegistry;
        }

        public List<Position> GetPossibleMoves(IPiece piece, List<IPiece> allPieces, GameField gameField, string[,] gameFieldString)
        {
            var pieceInfo = PieceInfo.FromPiece(piece);
            var boardQuery = new BoardQuery(allPieces, gameField, gameFieldString);
            var strategy = strategyRegistry.GetStrategy(pieceInfo.Type);
            return strategy.GetPossibleMoves(pieceInfo, boardQuery);
        }

        public List<Position> GetPossibleCaptures(IPiece piece, List<IPiece> allPieces, GameField gameField, string[,] gameFieldString)
        {
            var pieceInfo = PieceInfo.FromPiece(piece);
            var boardQuery = new BoardQuery(allPieces, gameField, gameFieldString);
            var strategy = strategyRegistry.GetStrategy(pieceInfo.Type);
            return strategy.GetPossibleCaptures(pieceInfo, boardQuery);
        }

        public List<Position> GetAllPossibleMoves(IPiece piece, List<IPiece> allPieces, GameField gameField, string[,] gameFieldString)
        {
            var moves = GetPossibleMoves(piece, allPieces, gameField, gameFieldString);
            var captures = GetPossibleCaptures(piece, allPieces, gameField, gameFieldString);
            return moves.Concat(captures).ToList();
        }
    }
}

