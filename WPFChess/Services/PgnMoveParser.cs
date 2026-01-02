using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ChessLib;

#nullable disable
namespace ChessWPF.Services
{
    /// <summary>
    /// Parser for converting PGN algebraic notation to actual moves
    /// </summary>
    public class PgnMoveParser
    {
        private static readonly Dictionary<char, int> FileMap = new Dictionary<char, int>
        {
            {'a', 0}, {'b', 1}, {'c', 2}, {'d', 3}, {'e', 4}, {'f', 5}, {'g', 6}, {'h', 7}
        };

        private static readonly Dictionary<char, int> RankMap = new Dictionary<char, int>
        {
            {'1', 0}, {'2', 1}, {'3', 2}, {'4', 3}, {'5', 4}, {'6', 5}, {'7', 6}, {'8', 7}
        };

        /// <summary>
        /// Parses a move in algebraic notation and finds the piece and destination
        /// </summary>
        public static MoveInfo ParseMove(string moveNotation, Game game)
        {
            if (string.IsNullOrWhiteSpace(moveNotation))
                return null;

            // Remove check/checkmate symbols
            moveNotation = moveNotation.Trim().TrimEnd('+', '#');

            // Handle castling
            if (moveNotation == "O-O" || moveNotation == "0-0")
            {
                return ParseCastling(game, true);
            }
            if (moveNotation == "O-O-O" || moveNotation == "0-0-0")
            {
                return ParseCastling(game, false);
            }

            // Handle promotion
            if (moveNotation.Contains("="))
            {
                // For now, skip promotion - would need special handling
                return null;
            }

            // Parse destination square
            var destMatch = Regex.Match(moveNotation, @"([a-h][1-8])$");
            if (!destMatch.Success)
                return null;

            var destSquare = destMatch.Groups[1].Value;
            var toX = FileMap[destSquare[0]];
            var toY = RankMap[destSquare[1]];
            var to = new ChessLib.Position(toX, toY);

            // Determine piece type
            char pieceChar = moveNotation[0];
            bool isCapture = moveNotation.Contains('x');
            
            IPiece piece = null;
            ChessLib.Position from = default;

            // Remove capture symbol and destination for parsing
            var movePart = moveNotation.Replace("x", "").Replace(destSquare, "");

            if (char.IsUpper(pieceChar) && pieceChar != 'O')
            {
                // Major piece (K, Q, R, B, N)
                piece = FindPieceByType(pieceChar, game, to, movePart);
            }
            else
            {
                // Pawn move
                piece = FindPawn(game, to, movePart, isCapture);
            }

            if (piece == null)
                return null;

            from = piece.Position;
            return new MoveInfo { From = from, To = to, Piece = piece };
        }

        private static MoveInfo ParseCastling(Game game, bool isShort)
        {
            var king = game.Pieces.FirstOrDefault(p => 
                p is King && 
                p.Color == game.CurrentPlayerColor && 
                !p.IsDead);

            if (king == null)
                return null;

            int rookX = isShort ? 7 : 0;
            var rook = game.Pieces.FirstOrDefault(p =>
                p is Rook &&
                p.Position.X == rookX &&
                p.Position.Y == king.Position.Y &&
                p.Color == king.Color &&
                !p.IsDead);

            if (rook == null)
                return null;

            int kingNewX = isShort ? 6 : 2;
            var to = new ChessLib.Position(kingNewX, king.Position.Y);

            return new MoveInfo { From = king.Position, To = to, Piece = king };
        }

        private static IPiece FindPieceByType(char pieceChar, Game game, ChessLib.Position to, string disambiguation)
        {
            Type pieceType = pieceChar switch
            {
                'K' => typeof(King),
                'Q' => typeof(Queen),
                'R' => typeof(Rook),
                'B' => typeof(Bishop),
                'N' => typeof(Knight),
                _ => null
            };

            if (pieceType == null)
                return null;

            var candidates = game.Pieces
                .Where(p => p.GetType() == pieceType &&
                           p.Color == game.CurrentPlayerColor &&
                           !p.IsDead)
                .ToList();

            if (candidates.Count == 0)
                return null;

            // If disambiguation info provided (file or rank)
            if (!string.IsNullOrEmpty(disambiguation))
            {
                foreach (var c in disambiguation)
                {
                    if (FileMap.ContainsKey(c))
                    {
                        candidates = candidates.Where(p => p.Position.X == FileMap[c]).ToList();
                    }
                    else if (RankMap.ContainsKey(c))
                    {
                        candidates = candidates.Where(p => p.Position.Y == RankMap[c]).ToList();
                    }
                }
            }

            // Find piece that can reach destination
            foreach (var candidate in candidates)
            {
                var validMoves = game.GetValidMoves(candidate);
                if (validMoves != null && validMoves.Contains(to))
                {
                    return candidate;
                }
            }

            return candidates.FirstOrDefault();
        }

        private static IPiece FindPawn(Game game, ChessLib.Position to, string disambiguation, bool isCapture)
        {
            var pawns = game.Pieces
                .Where(p => p is Pawn &&
                           p.Color == game.CurrentPlayerColor &&
                           !p.IsDead)
                .ToList();

            if (pawns.Count == 0)
                return null;

            // If file disambiguation provided (e.g., "exd5")
            if (!string.IsNullOrEmpty(disambiguation) && FileMap.ContainsKey(disambiguation[0]))
            {
                int file = FileMap[disambiguation[0]];
                pawns = pawns.Where(p => p.Position.X == file).ToList();
            }

            // Find pawn that can reach destination
            foreach (var pawn in pawns)
            {
                var validMoves = game.GetValidMoves(pawn);
                if (validMoves != null && validMoves.Contains(to))
                {
                    return pawn;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Information about a parsed move
    /// </summary>
    public class MoveInfo
    {
        public ChessLib.Position From { get; set; }
        public ChessLib.Position To { get; set; }
        public IPiece Piece { get; set; }
    }
}
