using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib.Services
{
    /// <summary>
    /// Validates chess moves according to rules
    /// </summary>
    public class MoveValidator
    {
        private readonly GameField gameField;

        public MoveValidator(GameField gameField)
        {
            this.gameField = gameField;
        }

        /// <summary>
        /// Checks if a move is valid
        /// </summary>
        public bool IsValidMove(List<IPiece> allPieces, IPiece piece, Position destination, string[,] gameFieldString)
        {
            if (piece == null || piece.IsDead)
                return false;

            // Check if destination is within board bounds
            if (destination.X < 0 || destination.X >= 8 || destination.Y < 0 || destination.Y >= 8)
                return false;

            // Get all possible moves for the piece
            var possibleMoves = piece.AvailableMoves(gameFieldString);
            var possibleKills = piece.AvailableKills(gameFieldString);
            var allPossibleMoves = possibleMoves.Concat(possibleKills).ToList();

            // Check if destination is in possible moves
            if (!allPossibleMoves.Contains(destination))
                return false;

            // Check if king is currently in check
            var king = allPieces.FirstOrDefault(p => p.Color == piece.Color && p is King && !p.IsDead) as King;
            bool isCurrentlyInCheck = false;
            if (king != null)
            {
                var enemyPieces = allPieces.Where(p => p.Color != piece.Color && !p.IsDead).ToList();
                isCurrentlyInCheck = gameField.GetAtackStatus(enemyPieces, king.Position, gameFieldString);
            }

            // Check if move would leave own king in check
            bool wouldCauseCheck = WouldMoveCauseCheck(allPieces, piece, destination, gameFieldString);

            // If king is currently in check, the move MUST remove the check (i.e., NOT cause check)
            if (isCurrentlyInCheck)
            {
                // Move is valid only if it removes the check
                return !wouldCauseCheck;
            }
            else
            {
                // If king is not in check, move is invalid if it would cause check
                if (wouldCauseCheck)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a move would cause check to own king
        /// </summary>
        public bool WouldMoveCauseCheck(List<IPiece> allPieces, IPiece piece, Position destination, string[,] gameFieldString)
        {
            // This is a simplified check - full implementation would simulate the move
            var currentPlayer = allPieces.FirstOrDefault(p => p.Color == piece.Color && p is King) as King;
            if (currentPlayer == null)
                return false;
            
            var player = new Player(piece.Color, allPieces.Where(p => p.Color == piece.Color).ToList());
            return gameField.GetCheckStatusAfterMove(allPieces, piece, destination, player);
        }

        /// <summary>
        /// Checks if a square is attacked by pieces of specified color
        /// </summary>
        public bool IsSquareAttacked(List<IPiece> pieces, Position square, PieceColor byColor, string[,] gameFieldString)
        {
            var attackingPieces = pieces.Where(p => p.Color == byColor && !p.IsDead).ToList();
            return gameField.GetAtackStatus(attackingPieces, square, gameFieldString);
        }

        /// <summary>
        /// Filters valid moves from possible moves, removing moves that would cause check
        /// </summary>
        public List<Position> FilterValidMoves(List<IPiece> allPieces, IPiece piece, List<Position> possibleMoves, string[,] gameFieldString)
        {
            var validMoves = new List<Position>();
            
            foreach (var move in possibleMoves)
            {
                if (IsValidMove(allPieces, piece, move, gameFieldString))
                {
                    validMoves.Add(move);
                }
            }

            return validMoves;
        }

        /// <summary>
        /// Checks if castling is possible
        /// </summary>
        public bool CanCastle(List<IPiece> allPieces, King king, Rook rook, CastleType castleType, string[,] gameFieldString)
        {
            if (king.IsMoved || rook.IsMoved)
                return false;

            if (king.Color != rook.Color)
                return false;

            var enemyPieces = allPieces.Where(p => p.Color != king.Color && !p.IsDead).ToList();
            var enemyColor = enemyPieces.FirstOrDefault()?.Color ?? (king.Color == PieceColor.White ? PieceColor.Black : PieceColor.White);

            // Check if king is currently in check
            if (IsSquareAttacked(allPieces, king.Position, enemyColor, gameFieldString))
                return false;

            // Check if squares between king and rook are free and not attacked
            int startX = castleType == CastleType.Short ? king.Position.X + 1 : king.Position.X - 1;
            int endX = castleType == CastleType.Short ? 7 : 0;
            int y = king.Position.Y;

            // Check all squares the king will pass through (including destination)
            int kingDestinationX = castleType == CastleType.Short ? 6 : 2;
            for (int x = startX; x != endX; x += castleType == CastleType.Short ? 1 : -1)
            {
                var position = new Position(x, y);
                
                // Check if square is free
                if (gameFieldString[x, y] != " ")
                    return false;

                // Check if square is attacked (king cannot pass through attacked squares)
                if (IsSquareAttacked(allPieces, position, enemyColor, gameFieldString))
                    return false;
            }

            // Also check the destination square
            var kingDest = new Position(kingDestinationX, y);
            if (IsSquareAttacked(allPieces, kingDest, enemyColor, gameFieldString))
                return false;

            return true;
        }
    }
}