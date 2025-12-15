using System.Collections.Generic;
using System.Linq;

namespace ChessLib
{
    /// <summary>
    /// Executes chess moves
    /// </summary>
    public class MoveExecutor
    {
        private readonly GameField gameField;

        public MoveExecutor(GameField gameField)
        {
            this.gameField = gameField;
        }

        /// <summary>
        /// Executes a move and returns the result
        /// </summary>
        public MoveResult ExecuteMove(List<IPiece> allPieces, IPiece piece, Position destination, string[,] gameFieldString)
        {
            if (piece == null || piece.IsDead)
                return MoveResult.Failure("Piece is null or dead");

            // Check if destination has an enemy piece (capture)
            var capturedPiece = allPieces.FirstOrDefault(p => p.Position == destination && p.Color != piece.Color && !p.IsDead);
            
            if (capturedPiece != null)
            {
                capturedPiece.IsDead = true;
                piece.ChangePosition(destination);
                return MoveResult.Success(MoveType.Capture);
            }

            // Regular move
            piece.ChangePosition(destination);
            return MoveResult.Success(MoveType.Normal);
        }

        /// <summary>
        /// Executes castling
        /// </summary>
        public MoveResult ExecuteCastling(List<IPiece> allPieces, King king, Rook rook, CastleType castleType)
        {
            if (king.IsMoved || rook.IsMoved)
                return MoveResult.Failure("King or rook has already moved");

            if (king.Color != rook.Color)
                return MoveResult.Failure("King and rook must be the same color");

            int kingNewX = castleType == CastleType.Short ? 6 : 2;
            int rookNewX = castleType == CastleType.Short ? 5 : 3;
            int y = king.Position.Y;

            king.ChangePosition(new Position(kingNewX, y));
            rook.ChangePosition(new Position(rookNewX, y));
            king.IsMoved = true;
            rook.IsMoved = true;

            return MoveResult.Success(MoveType.Castle);
        }

        /// <summary>
        /// Executes en passant capture
        /// </summary>
        public MoveResult ExecuteEnPassant(List<IPiece> allPieces, Pawn pawn, Position destination, Pawn capturedPawn)
        {
            if (capturedPawn == null || !capturedPawn.EnPassantAvailable)
                return MoveResult.Failure("En passant is not available");

            capturedPawn.IsDead = true;
            pawn.ChangePosition(destination);
            return MoveResult.Success(MoveType.EnPassant);
        }

        /// <summary>
        /// Removes dead pieces from the list
        /// </summary>
        public void RemoveDeadPieces(List<IPiece> pieces)
        {
            pieces.RemoveAll(p => p.IsDead);
        }
    }
}

