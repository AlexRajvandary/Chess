using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessLib
{
    public static class AlgebraicNotation
    {
        private static readonly string[] Files = ["a", "b", "c", "d", "e", "f", "g", "h"];
        private static readonly string[] Ranks = ["1", "2", "3", "4", "5", "6", "7", "8"];

        public static string ToAlgebraic(MoveNotation move, System.Collections.Generic.List<IPiece> allPieces)
        {
            var notation = new StringBuilder();

            if (move.MoveType == MoveType.Castle)
            {
                return move.To.X == 6 ? "O-O" : "O-O-O";
            }

            char pieceSymbol = GetPieceSymbol(move.Piece);
            if (pieceSymbol != 'P')
            {
                notation.Append(pieceSymbol);
            }

            if (move.Piece is Pawn)
            {
                if (move.MoveType == MoveType.Capture || move.MoveType == MoveType.EnPassant)
                {
                    notation.Append(Files[move.From.X]);
                }
            }
            else
            {
                var ambiguousPieces = allPieces
                    .Where(p => !p.IsDead && 
                                p.GetType() == move.Piece.GetType() && 
                                p.Color == move.Piece.Color &&
                                p != move.Piece)
                    .ToList();

                if (ambiguousPieces.Any(p => CanReachSquare(p, move.To, allPieces)))
                {
                    bool needFile = ambiguousPieces.Any(p => p.Position.X == move.From.X && p.Position.Y != move.From.Y);
                    bool needRank = ambiguousPieces.Any(p => p.Position.Y == move.From.Y && p.Position.X != move.From.X);

                    if (needFile || (!needRank && ambiguousPieces.Count > 0))
                    {
                        notation.Append(Files[move.From.X]);
                    }

                    if (needRank)
                    {
                        notation.Append(Ranks[move.From.Y]);
                    }
                }
            }

            if (move.MoveType == MoveType.Capture || move.MoveType == MoveType.EnPassant)
            {
                if (move.Piece is Pawn)
                {
                    notation.Append(Files[move.From.X]);
                }

                notation.Append('x');
            }

            notation.Append(Files[move.To.X]);
            notation.Append(Ranks[move.To.Y]);

            if (move.MoveType == MoveType.Promotion)
            {
                notation.Append("=Q");
            }

            if (move.IsCheckmate)
            {
                notation.Append('#');
            }
            else if (move.IsCheck)
            {
                notation.Append('+');
            }

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

        public static string FormatMoveHistory(List<MoveNotation> moves, List<IPiece> allPieces)
        {
            if (moves == null || moves.Count == 0)
            {
                return string.Empty;
            }

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
                result.Append(' ');

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