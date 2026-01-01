using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib.Strategies
{
    public class EnPassantStrategy : ISpecialMoveStrategy
    {
        public List<Position> GetPossibleSpecialMoves(IPiece piece, List<IPiece> allPieces, IBoardQuery board)
        {
            if (piece is not Pawn pawn)
            {
                return new List<Position>();
            }

            var enPassantMoves = new List<Position>();
            
            int pawnRank = pawn.Color == PieceColor.White ? 4 : 3;
            if (pawn.Position.Y != pawnRank)
            {
                return enPassantMoves;
            }

            var enemyPawns = allPieces
                .Where(p => p is Pawn p2 && 
                           p2.Color != pawn.Color && 
                           !p2.IsDead && 
                           ((Pawn)p2).EnPassantAvailable)
                .Cast<Pawn>()
                .ToList();

            foreach (var enemyPawn in enemyPawns)
            {
                int enemyRank = enemyPawn.Color == PieceColor.White ? 3 : 4;
                if (enemyPawn.Position.Y != enemyRank)
                {
                    continue;
                }

                if (pawn.Position.Y != enemyPawn.Position.Y)
                {
                    continue;
                }

                if (System.Math.Abs(pawn.Position.X - enemyPawn.Position.X) != 1)
                {
                    continue;
                }

                var destination = pawn.Color == PieceColor.White
                    ? new Position(enemyPawn.Position.X, enemyPawn.Position.Y + 1)
                    : new Position(enemyPawn.Position.X, enemyPawn.Position.Y - 1);

                if (destination.IsValid() && board.IsCellFree(destination))
                {
                    if (IsValidEnPassant(pawn, enemyPawn))
                    {
                        enPassantMoves.Add(destination);
                    }
                }
            }

            return enPassantMoves;
        }

        public bool CanExecuteSpecialMove(IPiece piece, Position destination, List<IPiece> allPieces, IBoardQuery board)
        {
            if (piece is not Pawn pawn)
            {
                return false;
            }

            if (!destination.IsValid() || !board.IsCellFree(destination))
            {
                return false;
            }

            var enemyPawns = allPieces
                .Where(p => p is Pawn p2 && 
                           p2.Color != pawn.Color && 
                           !p2.IsDead && 
                           ((Pawn)p2).EnPassantAvailable)
                .Cast<Pawn>()
                .ToList();

            foreach (var enemyPawn in enemyPawns)
            {
                int expectedY = pawn.Color == PieceColor.White 
                    ? enemyPawn.Position.Y + 1 
                    : enemyPawn.Position.Y - 1;

                if (destination.X == enemyPawn.Position.X && 
                    destination.Y == expectedY &&
                    pawn.Position.Y == enemyPawn.Position.Y &&
                    System.Math.Abs(pawn.Position.X - enemyPawn.Position.X) == 1)
                {
                    return IsValidEnPassant(pawn, enemyPawn);
                }
            }

            return false;
        }

        private static bool IsValidEnPassant(Pawn pawn, Pawn enemyPawn)
        {
            var enemyTargetPos = new Position(
                enemyPawn.StartPos.X + GetEnemyMoveDir(enemyPawn.Color)[1].X,
                enemyPawn.StartPos.Y + GetEnemyMoveDir(enemyPawn.Color)[1].Y);

            bool isEnemyPositionCorrect = enemyPawn.Position == enemyTargetPos;
            bool isVerticalPositionCorrect = pawn.Position.Y == enemyPawn.Position.Y;
            bool isHorizontalPosition1Correct = pawn.Position.X == enemyPawn.Position.X + 1;
            bool isHorizontalPosition2Correct = pawn.Position.X == enemyPawn.Position.X - 1;

            return enemyPawn.EnPassantAvailable && 
                   isEnemyPositionCorrect && 
                   (isHorizontalPosition1Correct || isHorizontalPosition2Correct) && 
                   isVerticalPositionCorrect;
        }

        private static List<Position> GetEnemyMoveDir(PieceColor enemyColor)
        {
            return enemyColor == PieceColor.White 
                ? new List<Position> { new Position(0, 1), new Position(0, 2) }
                : new List<Position> { new Position(0, -1), new Position(0, -2) };
        }
    }
}

