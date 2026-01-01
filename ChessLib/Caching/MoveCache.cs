using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib.Caching
{
    public class MoveCache : IMoveCache
    {
        private readonly Dictionary<string, List<Position>> _movesCache = new();
        private readonly Dictionary<string, List<Position>> _capturesCache = new();

        public List<Position> GetMoves(PieceType pieceType, PieceColor color, Position position, string boardState)
        {
            var key = CreateKey(pieceType, color, position, boardState);
            return _movesCache.TryGetValue(key, out var moves) ? moves.ToList() : null;
        }

        public List<Position> GetCaptures(PieceType pieceType, PieceColor color, Position position, string boardState)
        {
            var key = CreateKey(pieceType, color, position, boardState);
            return _capturesCache.TryGetValue(key, out var captures) ? captures.ToList() : null;
        }

        public void SetMoves(PieceType pieceType, PieceColor color, Position position, string boardState, List<Position> moves)
        {
            var key = CreateKey(pieceType, color, position, boardState);
            _movesCache[key] = moves.ToList();
        }

        public void SetCaptures(PieceType pieceType, PieceColor color, Position position, string boardState, List<Position> captures)
        {
            var key = CreateKey(pieceType, color, position, boardState);
            _capturesCache[key] = captures.ToList();
        }

        public void Clear()
        {
            _movesCache.Clear();
            _capturesCache.Clear();
        }

        private static string CreateKey(PieceType pieceType, PieceColor color, Position position, string boardState)
        {
            return $"{pieceType}:{color}:{position.X},{position.Y}:{boardState}";
        }
    }
}

