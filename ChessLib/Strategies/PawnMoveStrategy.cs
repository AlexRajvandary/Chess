using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;

namespace ChessLib.Strategies
{
    public class PawnMoveStrategy : IMoveStrategy
    {
        public PieceType PieceType => PieceType.Pawn;

        public List<Position> GetPossibleMoves(IPieceInfo piece, IBoardQuery board)
        {
            var moves = new List<Position>();
            var forwardDirection = GetForwardDirection(piece.Color);
            var startRank = GetStartRank(piece.Color);

            var oneStep = new Position(piece.Position.X + forwardDirection.X, piece.Position.Y + forwardDirection.Y);
            if (oneStep.IsValid() && board.IsCellFree(oneStep))
            {
                moves.Add(oneStep);

                if (piece.Position.Y == startRank)
                {
                    var twoStep = new Position(piece.Position.X + forwardDirection.X * 2, piece.Position.Y + forwardDirection.Y * 2);
                    if (twoStep.IsValid() && board.IsCellFree(twoStep))
                    {
                        moves.Add(twoStep);
                    }
                }
            }

            return moves;
        }

        public List<Position> GetPossibleCaptures(IPieceInfo piece, IBoardQuery board)
        {
            var captures = new List<Position>();
            var forwardDirection = GetForwardDirection(piece.Color);
            var attackDirections = GetAttackDirections(piece.Color);

            foreach (var attackDir in attackDirections)
            {
                var capturePos = new Position(piece.Position.X + attackDir.X, piece.Position.Y + attackDir.Y);
                if (capturePos.IsValid())
                {
                    var targetPiece = board.GetPieceAt(capturePos);
                    if (targetPiece != null && targetPiece.Color != piece.Color)
                    {
                        captures.Add(capturePos);
                    }
                }
            }

            return captures;
        }

        private static Position GetForwardDirection(PieceColor color)
        {
            return color == PieceColor.White ? new Position(0, 1) : new Position(0, -1);
        }

        private static int GetStartRank(PieceColor color)
        {
            return color == PieceColor.White ? 1 : 6;
        }

        private static List<Position> GetAttackDirections(PieceColor color)
        {
            if (color == PieceColor.White)
            {
                return new List<Position> { new Position(-1, 1), new Position(1, 1) };
            }
            else
            {
                return new List<Position> { new Position(-1, -1), new Position(1, -1) };
            }
        }
    }
}

