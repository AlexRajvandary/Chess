using ChessLib.Caching;
using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessLib.Services
{
    public class MoveStrategyService
    {
        private readonly IMoveStrategyRegistry strategyRegistry;
        private readonly IMoveCache cache;

        public MoveStrategyService(IMoveStrategyRegistry strategyRegistry, IMoveCache cache = null)
        {
            this.strategyRegistry = strategyRegistry;
            this.cache = cache;
        }

        public List<Position> GetPossibleMoves(IPiece piece, List<IPiece> allPieces, GameField gameField, string[,] gameFieldString)
        {
            var pieceInfo = PieceInfo.FromPiece(piece);
            var boardState = GetBoardState(gameFieldString);
            
            if (cache != null)
            {
                var cachedMoves = cache.GetMoves(pieceInfo.Type, pieceInfo.Color, pieceInfo.Position, boardState);
                if (cachedMoves != null)
                {
                    return cachedMoves;
                }
            }

            var boardQuery = new BoardQuery(allPieces, gameField, gameFieldString);
            var strategy = strategyRegistry.GetStrategy(pieceInfo.Type);
            var moves = strategy.GetPossibleMoves(pieceInfo, boardQuery);
            
            if (cache != null)
            {
                cache.SetMoves(pieceInfo.Type, pieceInfo.Color, pieceInfo.Position, boardState, moves);
            }
            
            return moves;
        }

        public List<Position> GetPossibleCaptures(IPiece piece, List<IPiece> allPieces, GameField gameField, string[,] gameFieldString)
        {
            var pieceInfo = PieceInfo.FromPiece(piece);
            var boardState = GetBoardState(gameFieldString);
            
            if (cache != null)
            {
                var cachedCaptures = cache.GetCaptures(pieceInfo.Type, pieceInfo.Color, pieceInfo.Position, boardState);
                if (cachedCaptures != null)
                {
                    return cachedCaptures;
                }
            }

            var boardQuery = new BoardQuery(allPieces, gameField, gameFieldString);
            var strategy = strategyRegistry.GetStrategy(pieceInfo.Type);
            var captures = strategy.GetPossibleCaptures(pieceInfo, boardQuery);
            
            if (cache != null)
            {
                cache.SetCaptures(pieceInfo.Type, pieceInfo.Color, pieceInfo.Position, boardState, captures);
            }
            
            return captures;
        }

        private static string GetBoardState(string[,] gameFieldString)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    sb.Append(gameFieldString[i, j] ?? " ");
                }
            }
            return sb.ToString();
        }

        public List<Position> GetAllPossibleMoves(IPiece piece, List<IPiece> allPieces, GameField gameField, string[,] gameFieldString)
        {
            var moves = GetPossibleMoves(piece, allPieces, gameField, gameFieldString);
            var captures = GetPossibleCaptures(piece, allPieces, gameField, gameFieldString);
            return moves.Concat(captures).ToList();
        }
    }
}

