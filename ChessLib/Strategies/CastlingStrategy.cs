using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib.Strategies
{
    public class CastlingStrategy : ISpecialMoveStrategy
    {
        public List<Position> GetPossibleSpecialMoves(IPiece piece, List<IPiece> allPieces, IBoardQuery board)
        {
            if (piece is not King king || king.IsMoved)
            {
                return new List<Position>();
            }

            var castlingMoves = new List<Position>();
            int y = king.Position.Y;

            var rooks = allPieces
                .Where(p => p is Rook r && 
                           r.Color == king.Color && 
                           !r.IsMoved && 
                           !r.IsDead &&
                           r.Position.Y == y)
                .Cast<Rook>()
                .ToList();

            foreach (var rook in rooks)
            {
                if (rook.Position.X == 7)
                {
                    if (CanCastle(king, rook, CastleType.Short, allPieces, board))
                    {
                        castlingMoves.Add(new Position(6, y));
                    }
                }
                else if (rook.Position.X == 0)
                {
                    if (CanCastle(king, rook, CastleType.Long, allPieces, board))
                    {
                        castlingMoves.Add(new Position(2, y));
                    }
                }
            }

            return castlingMoves;
        }

        public bool CanExecuteSpecialMove(IPiece piece, Position destination, List<IPiece> allPieces, IBoardQuery board)
        {
            if (piece is not King king || king.IsMoved)
            {
                return false;
            }

            int y = king.Position.Y;
            CastleType? castleType = null;
            Rook targetRook = null;

            if (destination.X == 6 && destination.Y == y)
            {
                castleType = CastleType.Short;
                targetRook = allPieces
                    .FirstOrDefault(p => p is Rook r && 
                                        r.Color == king.Color && 
                                        !r.IsMoved && 
                                        !r.IsDead && 
                                        r.Position.X == 7 && 
                                        r.Position.Y == y) as Rook;
            }
            else if (destination.X == 2 && destination.Y == y)
            {
                castleType = CastleType.Long;
                targetRook = allPieces
                    .FirstOrDefault(p => p is Rook r && 
                                        r.Color == king.Color && 
                                        !r.IsMoved && 
                                        !r.IsDead && 
                                        r.Position.X == 0 && 
                                        r.Position.Y == y) as Rook;
            }

            if (castleType == null || targetRook == null)
            {
                return false;
            }

            return CanCastle(king, targetRook, castleType.Value, allPieces, board);
        }

        private bool CanCastle(King king, Rook rook, CastleType castleType, List<IPiece> allPieces, IBoardQuery board)
        {
            if (king.IsMoved || rook.IsMoved)
                return false;

            if (king.Color != rook.Color)
                return false;

            var enemyPieces = allPieces.Where(p => p.Color != king.Color && !p.IsDead).ToList();
            var enemyColor = enemyPieces.FirstOrDefault()?.Color ?? (king.Color == PieceColor.White ? PieceColor.Black : PieceColor.White);

            if (board.IsCellAttacked(king.Position, enemyColor))
                return false;

            int y = king.Position.Y;
            int kingStartX = 4;
            int kingEndX = castleType == CastleType.Short ? 6 : 2;
            int rookX = castleType == CastleType.Short ? 7 : 0;

            int startX = System.Math.Min(kingStartX, kingEndX);
            int endX = System.Math.Max(kingStartX, kingEndX);

            for (int x = startX; x <= endX; x++)
            {
                var pos = new Position(x, y);
                if (x != kingStartX && x != rookX && !board.IsCellFree(pos))
                {
                    return false;
                }

                if (x != rookX && board.IsCellAttacked(pos, enemyColor))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

