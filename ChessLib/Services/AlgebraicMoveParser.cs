using ChessLib.Common;
using ChessLib.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChessLib.Services
{
    public class AlgebraicMoveParser
    {
        private static readonly Dictionary<char, int> FileMap = new Dictionary<char, int>
        {
            {'a', 0}, {'b', 1}, {'c', 2}, {'d', 3}, {'e', 4}, {'f', 5}, {'g', 6}, {'h', 7}
        };

        private static readonly Dictionary<char, int> RankMap = new Dictionary<char, int>
        {
            {'1', 0}, {'2', 1}, {'3', 2}, {'4', 3}, {'5', 4}, {'6', 5}, {'7', 6}, {'8', 7}
        };

        public static ParsedMove ParseMove(string moveNotation, IGameEngine gameEngine)
        {
            if (string.IsNullOrWhiteSpace(moveNotation))
                return null;

            moveNotation = moveNotation.Trim().TrimEnd('+', '#');

            if (moveNotation == "O-O" || moveNotation == "0-0")
            {
                return ParseCastling(gameEngine, true);
            }
            if (moveNotation == "O-O-O" || moveNotation == "0-0-0")
            {
                return ParseCastling(gameEngine, false);
            }

            if (moveNotation.Contains("="))
            {
                return null;
            }

            var destMatch = Regex.Match(moveNotation, @"([a-h][1-8])$");
            if (!destMatch.Success)
                return null;

            var destSquare = destMatch.Groups[1].Value;
            var toX = FileMap[destSquare[0]];
            var toY = RankMap[destSquare[1]];
            var to = new Position(toX, toY);

            char pieceChar = moveNotation[0];
            bool isCapture = moveNotation.Contains('x');
            
            var movePart = moveNotation.Replace("x", "").Replace(destSquare, "");

            Position? from = null;

            if (char.IsUpper(pieceChar) && pieceChar != 'O')
            {
                from = FindPieceByType(pieceChar, gameEngine, to, movePart);
            }
            else
            {
                from = FindPawn(gameEngine, to, movePart, isCapture);
            }

            if (from == null)
                return null;

            return new ParsedMove { From = from.Value, To = to };
        }

        private static ParsedMove ParseCastling(IGameEngine gameEngine, bool isShort)
        {
            var state = gameEngine.GetState();
            var king = state.Pieces.FirstOrDefault(p => 
                p.Type == PieceType.King && 
                p.Color == state.CurrentPlayerColor);

            if (king == null)
                return null;

            int rookX = isShort ? 7 : 0;
            var rook = state.Pieces.FirstOrDefault(p =>
                p.Type == PieceType.Rook &&
                p.Position.X == rookX &&
                p.Position.Y == king.Position.Y &&
                p.Color == king.Color);

            if (rook == null)
                return null;

            int kingNewX = isShort ? 6 : 2;
            var to = new Position(kingNewX, king.Position.Y);

            return new ParsedMove { From = king.Position, To = to };
        }

        private static Position? FindPieceByType(char pieceChar, IGameEngine gameEngine, Position to, string disambiguation)
        {
            PieceType? pieceType = pieceChar switch
            {
                'K' => PieceType.King,
                'Q' => PieceType.Queen,
                'R' => PieceType.Rook,
                'B' => PieceType.Bishop,
                'N' => PieceType.Knight,
                _ => null
            };

            if (pieceType == null)
                return null;

            var state = gameEngine.GetState();
            var candidates = state.Pieces
                .Where(p => p.Type == pieceType.Value &&
                           p.Color == state.CurrentPlayerColor)
                .ToList();

            if (candidates.Count == 0)
                return null;

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

            foreach (var candidate in candidates)
            {
                var validMoves = gameEngine.GetValidMoves(candidate.Position);
                if (validMoves != null && validMoves.Contains(to))
                {
                    return candidate.Position;
                }
            }

            return candidates.FirstOrDefault()?.Position;
        }

        private static Position? FindPawn(IGameEngine gameEngine, Position to, string disambiguation, bool isCapture)
        {
            var state = gameEngine.GetState();
            var pawns = state.Pieces
                .Where(p => p.Type == PieceType.Pawn &&
                           p.Color == state.CurrentPlayerColor)
                .ToList();

            if (pawns.Count == 0)
                return null;

            if (!string.IsNullOrEmpty(disambiguation) && FileMap.ContainsKey(disambiguation[0]))
            {
                int file = FileMap[disambiguation[0]];
                pawns = pawns.Where(p => p.Position.X == file).ToList();
            }

            foreach (var pawn in pawns)
            {
                var validMoves = gameEngine.GetValidMoves(pawn.Position);
                if (validMoves != null && validMoves.Contains(to))
                {
                    return pawn.Position;
                }
            }

            return pawns.FirstOrDefault()?.Position;
        }
    }

    public class ParsedMove
    {
        public Position From { get; set; }
        public Position To { get; set; }
    }
}

