using System.Linq;
using System.Text;

namespace ChessLib
{
    /// <summary>
    /// FEN (Forsyth-Edwards Notation) notation generator
    /// </summary>
    public static class Fen
    {
        /// <summary>
        /// Converts piece to FEN character
        /// </summary>
        private static char GetPieceChar(IPiece piece)
        {
            if (piece == null || piece.IsDead)
            {
                return ' ';
            }

            char pieceChar = piece switch
            {
                Pawn => 'p',
                Rook => 'r',
                Knight => 'n',
                Bishop => 'b',
                Queen => 'q',
                King => 'k',
                _ => ' '
            };

            return piece.Color == PieceColor.White ? char.ToUpper(pieceChar) : pieceChar;
        }

        /// <summary>
        /// Generates FEN notation from game state
        /// Format: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        /// </summary>
        public static string GenerateFen(Game game)
        {
            var state = game.GetState();
            var fen = new StringBuilder();

            // 1. Piece placement (ranks 8 to 1, from top to bottom)
            for (int row = 7; row >= 0; row--)
            {
                int emptyCount = 0;

                for (int col = 0; col < 8; col++)
                {
                    var piece = state.Pieces.FirstOrDefault(p => p.Position.X == col && p.Position.Y == row && !p.IsDead);
                    
                    if (piece != null)
                    {
                        if (emptyCount > 0)
                        {
                            fen.Append(emptyCount);
                            emptyCount = 0;
                        }
                        fen.Append(GetPieceChar(piece));
                    }
                    else
                    {
                        emptyCount++;
                    }
                }

                if (emptyCount > 0)
                {
                    fen.Append(emptyCount);
                }

                if (row > 0)
                {
                    fen.Append('/');
                }
            }

            // 2. Active color
            fen.Append(' ');
            fen.Append(state.CurrentPlayerColor == PieceColor.White ? 'w' : 'b');

            // 3. Castling rights
            fen.Append(' ');
            string castling = GetCastlingRights(state.Pieces);
            fen.Append(castling);

            // 4. En passant target square
            fen.Append(' ');
            string enPassant = GetEnPassantSquare(state.Pieces, state.CurrentPlayerColor);
            fen.Append(enPassant);

            // 5. Halfmove clock (for 50-move rule) - simplified to 0 for now
            fen.Append(" 0");

            // 6. Fullmove number - simplified to 1 for now
            fen.Append(" 1");

            return fen.ToString();
        }

        private static string GetCastlingRights(System.Collections.Generic.List<IPiece> pieces)
        {
            var castling = new StringBuilder();

            // White king and rooks
            if (pieces.FirstOrDefault(p => p is King && p.Color == PieceColor.White && !p.IsDead) is King whiteKing && !whiteKing.IsMoved)
            {
                var whiteRooks = pieces.Where(p => p is Rook && p.Color == PieceColor.White && !p.IsDead).Cast<Rook>().ToList();
                var kingsideRook = whiteRooks.FirstOrDefault(r => r.RookKind == RookKind.Royal && !r.IsMoved);
                var queensideRook = whiteRooks.FirstOrDefault(r => r.RookKind == RookKind.Queen && !r.IsMoved);

                if (kingsideRook != null)
                    castling.Append('K');
                if (queensideRook != null)
                    castling.Append('Q');
            }

            // Black king and rooks
            if (pieces.FirstOrDefault(p => p is King && p.Color == PieceColor.Black && !p.IsDead) is King blackKing && !blackKing.IsMoved)
            {
                var blackRooks = pieces.Where(p => p is Rook && p.Color == PieceColor.Black && !p.IsDead).Cast<Rook>().ToList();
                var kingsideRook = blackRooks.FirstOrDefault(r => r.RookKind == RookKind.Royal && !r.IsMoved);
                var queensideRook = blackRooks.FirstOrDefault(r => r.RookKind == RookKind.Queen && !r.IsMoved);

                if (kingsideRook != null)
                    castling.Append('k');
                if (queensideRook != null)
                    castling.Append('q');
            }

            return castling.Length > 0 ? castling.ToString() : "-";
        }

        /// <summary>
        /// Gets en passant target square in FEN format
        /// </summary>
        private static string GetEnPassantSquare(System.Collections.Generic.List<IPiece> pieces, PieceColor currentPlayerColor)
        {
            // Find pawn that can be captured en passant (opposite color, EnPassantAvailable = true)
            // The en passant target square is the square the pawn would move to if captured
            var enemyColor = currentPlayerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            var enemyPawns = pieces.Where(p => p is Pawn && p.Color == enemyColor && !p.IsDead).Cast<Pawn>();

            var enPassantPawn = enemyPawns.FirstOrDefault(p => p.EnPassantAvailable);
            if (enPassantPawn == null)
                return "-";

            // En passant target square is the square the pawn would move to if captured
            // For white pawns (on rank 4, moved from rank 2), target is rank 3 (Y=2)
            // For black pawns (on rank 3, moved from rank 6), target is rank 4 (Y=3)
            // The target square is horizontally adjacent to the pawn, one rank forward from the pawn's starting position
            int targetY = enemyColor == PieceColor.White ? enPassantPawn.Position.Y - 1 : enPassantPawn.Position.Y + 1;
            char file = (char)('a' + enPassantPawn.Position.X);
            char rank = (char)('1' + targetY);

            return $"{file}{rank}";
        }
    }
}