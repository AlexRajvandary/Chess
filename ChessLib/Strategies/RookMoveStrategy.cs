using ChessLib.Common;
using System.Collections.Generic;

namespace ChessLib.Strategies
{
    public class RookMoveStrategy : IMoveStrategy
    {
        public PieceType PieceType => PieceType.Rook;

        private static readonly Position[] Directions = new Position[]
        {
            new Position(0, 1), new Position(0, -1), new Position(1, 0), new Position(-1, 0)
        };

        public List<Position> GetPossibleMoves(IPieceInfo piece, IBoardQuery board)
        {
            var moves = new List<Position>();

            foreach (var direction in Directions)
            {
                for (int i = 1; i < 8; i++)
                {
                    var newPos = new Position(piece.Position.X + direction.X * i, piece.Position.Y + direction.Y * i);
                    if (!newPos.IsValid())
                        break;

                    if (board.IsCellFree(newPos))
                    {
                        moves.Add(newPos);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return moves;
        }

        public List<Position> GetPossibleCaptures(IPieceInfo piece, IBoardQuery board)
        {
            var captures = new List<Position>();

            foreach (var direction in Directions)
            {
                for (int i = 1; i < 8; i++)
                {
                    var newPos = new Position(piece.Position.X + direction.X * i, piece.Position.Y + direction.Y * i);
                    if (!newPos.IsValid())
                        break;

                    var targetPiece = board.GetPieceAt(newPos);
                    if (targetPiece != null)
                    {
                        if (targetPiece.Color != piece.Color)
                        {
                            captures.Add(newPos);
                        }
                        break;
                    }
                }
            }

            return captures;
        }
    }
}

