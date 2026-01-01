using ChessLib.Common;
using System.Collections.Generic;

namespace ChessLib.Strategies
{
    public class KingMoveStrategy : IMoveStrategy
    {
        public PieceType PieceType => PieceType.King;

        private static readonly Position[] Directions = new Position[]
        {
            new Position(1, 1), new Position(0, 1), new Position(-1, 1), new Position(-1, 0),
            new Position(1, 0), new Position(-1, -1), new Position(0, -1), new Position(1, -1)
        };

        public List<Position> GetPossibleMoves(IPieceInfo piece, IBoardQuery board)
        {
            var moves = new List<Position>();

            foreach (var direction in Directions)
            {
                var newPos = new Position(piece.Position.X + direction.X, piece.Position.Y + direction.Y);
                if (newPos.IsValid() && board.IsCellFree(newPos))
                {
                    moves.Add(newPos);
                }
            }

            return moves;
        }

        public List<Position> GetPossibleCaptures(IPieceInfo piece, IBoardQuery board)
        {
            var captures = new List<Position>();

            foreach (var direction in Directions)
            {
                var newPos = new Position(piece.Position.X + direction.X, piece.Position.Y + direction.Y);
                if (newPos.IsValid())
                {
                    var targetPiece = board.GetPieceAt(newPos);
                    if (targetPiece != null && targetPiece.Color != piece.Color)
                    {
                        captures.Add(newPos);
                    }
                }
            }

            return captures;
        }
    }
}

