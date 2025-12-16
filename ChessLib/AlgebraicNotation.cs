using System.Linq;
using System.Text;

namespace ChessLib
{
    /// <summary>
    /// Converts moves to algebraic notation (e.g., "e4", "Nf3", "O-O", "Qxd5+")
    /// </summary>
    public static class AlgebraicNotation
    {
        private static readonly string[] Files = { "a", "b", "c", "d", "e", "f", "g", "h" };
        private static readonly string[] Ranks = { "1", "2", "3", "4", "5", "6", "7", "8" };

        /// <summary>
        /// Converts a move to algebraic notation
        /// </summary>
        public static string ToAlgebraic(MoveNotation move, System.Collections.Generic.List<IPiece> allPieces)
        {
            var notation = new StringBuilder();

            // Castling
            if (move.MoveType == MoveType.Castle)
            {
                if (move.To.X == 6) // Kingside
                    return "O-O";
                else // Queenside
                    return "O-O-O";
            }

            // Get piece symbol
            char pieceSymbol = GetPieceSymbol(move.Piece);
            if (pieceSymbol != 'P')
            {
                notation.Append(pieceSymbol);
            }

            // For pawns and ambiguous moves, add source file/rank if needed
            if (move.Piece is Pawn)
            {
                // For captures, add source file
                if (move.MoveType == MoveType.Capture || move.MoveType == MoveType.EnPassant)
                {
                    notation.Append(Files[move.From.X]);
                }
            }
            else
            {
                // For other pieces, check for ambiguity
                var ambiguousPieces = allPieces
                    .Where(p => !p.IsDead && 
                                p.GetType() == move.Piece.GetType() && 
                                p.Color == move.Piece.Color &&
                                p != move.Piece)
                    .ToList();

                if (ambiguousPieces.Any(p => CanReachSquare(p, move.To, allPieces)))
                {
                    // Add disambiguation (file, rank, or both)
                    bool needFile = ambiguousPieces.Any(p => p.Position.X == move.From.X && p.Position.Y != move.From.Y);
                    bool needRank = ambiguousPieces.Any(p => p.Position.Y == move.From.Y && p.Position.X != move.From.X);

                    if (needFile || (!needRank && ambiguousPieces.Count > 0))
                        notation.Append(Files[move.From.X]);
                    if (needRank)
                        notation.Append(Ranks[move.From.Y]);
                }
            }

            // Capture notation
            if (move.MoveType == MoveType.Capture || move.MoveType == MoveType.EnPassant)
            {
                if (move.Piece is Pawn)
                    notation.Append(Files[move.From.X]); // Already added above for pawns
                notation.Append('x');
            }

            // Destination square
            notation.Append(Files[move.To.X]);
            notation.Append(Ranks[move.To.Y]);

            // Promotion (simplified - would need more info)
            if (move.MoveType == MoveType.Promotion)
            {
                notation.Append("=Q"); // Default to queen
            }

            // Check/Checkmate
            if (move.IsCheckmate)
                notation.Append('#');
            else if (move.IsCheck)
                notation.Append('+');

            return notation.ToString();
        }

        private static char GetPieceSymbol(IPiece piece)
        {
            return piece switch
            {
                King => 'K',
                Queen => 'Q',
                Rook => 'R',
                Bishop => 'B',
                Knight => 'N',
                Pawn => 'P',
                _ => '?'
            };
        }

        private static bool CanReachSquare(IPiece piece, Position square, System.Collections.Generic.List<IPiece> allPieces)
        {
            // Simplified check - in real implementation would need to check actual moves
            // For now, just check if piece type matches
            return true; // Simplified
        }

        /// <summary>
        /// Formats move history in standard notation (e.g., "1. e4 e5 2. Nf3 Nc6")
        /// </summary>
        public static string FormatMoveHistory(System.Collections.Generic.List<MoveNotation> moves, System.Collections.Generic.List<IPiece> allPieces)
        {
            if (moves == null || moves.Count == 0)
                return "";

            var result = new StringBuilder();
            int currentMoveNumber = 1;
            bool isWhiteMove = true;

            foreach (var move in moves)
            {
                if (isWhiteMove)
                {
                    result.Append($"{currentMoveNumber}. ");
                }

                result.Append(ToAlgebraic(move, allPieces));
                result.Append(" ");

                if (!isWhiteMove)
                {
                    currentMoveNumber++;
                }

                isWhiteMove = !isWhiteMove;
            }

            return result.ToString().Trim();
        }
    }
}

