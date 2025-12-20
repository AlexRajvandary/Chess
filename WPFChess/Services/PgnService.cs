using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessLib;

#nullable disable
namespace ChessWPF.Services
{
    /// <summary>
    /// Service for working with PGN (Portable Game Notation) format
    /// </summary>
    public class PgnService
    {
        private const string classicStartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public string GeneratePgn(Game game,
                                  string whitePlayer = "White",
                                  string blackPlayer = "Black", 
                                  string eventName = null,
                                  string site = null,
                                  string round = null)
        {
            var pgn = new StringBuilder();

            pgn.AppendLine($"[Event \"{eventName ?? "Casual Game"}\"]");
            pgn.AppendLine($"[Site \"{site ?? "?"}\"]");
            pgn.AppendLine($"[Date \"{DateTime.Now:yyyy.MM.dd}\"]");
            pgn.AppendLine($"[Round \"{round ?? "?"}\"]");
            pgn.AppendLine($"[White \"{whitePlayer}\"]");
            pgn.AppendLine($"[Black \"{blackPlayer}\"]");

            string result = "*";
            if (game.IsGameOver)
            {
                var lastMove = game.MoveHistory.LastOrDefault();
                if (lastMove?.IsCheckmate == true)
                {
                    result = lastMove.PlayerColor == PieceColor.White ? "1-0" : "0-1";
                }
                else
                {
                    result = "1/2-1/2";
                }
            }
            pgn.AppendLine($"[Result \"{result}\"]");

            var initialFen = game.GetFen();
            if (initialFen != classicStartPositionFen)
            {
                pgn.AppendLine($"[FEN \"{initialFen}\"]");
            }

            pgn.AppendLine();

            // Moves in algebraic notation
            var moveHistory = game.GetMoveHistory();
            if (!string.IsNullOrEmpty(moveHistory))
            {
                pgn.Append(moveHistory);
            }

            pgn.Append($" {result}");

            return pgn.ToString();
        }

        public static List<string> ParsePgnMoves(string pgn)
        {
            var moves = new List<string>();
            var lines = pgn.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var inMovesSection = false;
            var movesText = new StringBuilder();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                // Skip header lines
                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    continue;
                }

                // Empty line marks end of headers and start of moves
                if (string.IsNullOrWhiteSpace(trimmed) && !inMovesSection)
                {
                    inMovesSection = true;
                    continue;
                }

                if (inMovesSection)
                {
                    movesText.Append(trimmed);
                    movesText.Append(" ");
                }
            }

            var movesString = movesText.ToString().Trim();
            if (string.IsNullOrEmpty(movesString))
            {
                return moves;
            }

            // Remove result at the end (1-0, 0-1, 1/2-1/2, *)
            movesString = System.Text.RegularExpressions.Regex.Replace(movesString, @"\s+(1-0|0-1|1/2-1/2|\*)\s*$", "");

            // Normalize whitespace
            movesString = System.Text.RegularExpressions.Regex.Replace(movesString, @"\s+", " ");

            // Split by move numbers (e.g., "1. ", "2. ", "1.d4", etc.)
            // Pattern: number, dot, optional space, then moves until next number or end
            // This pattern handles both "1. e4" and "1.e4" formats
            var movePattern = @"(\d+)\.\s*([^\d]+?)(?=\s*\d+\.|$)";
            var matches = System.Text.RegularExpressions.Regex.Matches(movesString, movePattern);
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    // Group 2 contains the moves for this move number (white and black)
                    var movePair = match.Groups[2].Value.Trim();
                    
                    // Split by spaces to get individual moves
                    var individualMoves = movePair.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var move in individualMoves)
                    {
                        var cleanMove = move.Trim();
                        
                        // Skip if it's a move number (e.g., "1.", "2.", "1", "2")
                        if (System.Text.RegularExpressions.Regex.IsMatch(cleanMove, @"^\d+\.?$"))
                        {
                            continue;
                        }
                        
                        // Skip if it starts with a number and dot (e.g., "1.d4" should become "d4")
                        var numberDotMatch = System.Text.RegularExpressions.Regex.Match(cleanMove, @"^(\d+)\.(.+)$");
                        if (numberDotMatch.Success && numberDotMatch.Groups.Count >= 3)
                        {
                            cleanMove = numberDotMatch.Groups[2].Value.Trim();
                        }
                        
                        // Skip results
                        if (cleanMove == "1-0" || cleanMove == "0-1" || cleanMove == "1/2-1/2" || cleanMove == "*")
                        {
                            continue;
                        }
                        
                        // Remove check/checkmate symbols for parsing
                        cleanMove = cleanMove.TrimEnd('+', '#');
                        
                        if (!string.IsNullOrEmpty(cleanMove))
                        {
                            moves.Add(cleanMove);
                        }
                    }
                }
            }

            // If no matches found with the pattern, try simpler approach
            // Split by spaces and filter out move numbers
            if (moves.Count == 0)
            {
                var parts = movesString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    
                    // Skip if it's a move number (e.g., "1.", "2.", "1", "2")
                    if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d+\.?$"))
                    {
                        continue;
                    }
                    
                    // Skip if it starts with a number and dot (e.g., "1.d4" should become "d4")
                    var numberDotMatch = System.Text.RegularExpressions.Regex.Match(trimmed, @"^(\d+)\.(.+)$");
                    if (numberDotMatch.Success && numberDotMatch.Groups.Count >= 3)
                    {
                        trimmed = numberDotMatch.Groups[2].Value.Trim();
                    }
                    
                    // Skip results
                    if (trimmed == "1-0" || trimmed == "0-1" || trimmed == "1/2-1/2" || trimmed == "*")
                    {
                        continue;
                    }
                    
                    // Remove check/checkmate symbols
                    trimmed = trimmed.TrimEnd('+', '#');
                    
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        moves.Add(trimmed);
                    }
                }
            }

            return moves;
        }

        public static Dictionary<string, string> ParsePgnHeaders(string pgn)
        {
            var headers = new Dictionary<string, string>();
            var lines = pgn.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    var content = trimmed.Substring(1, trimmed.Length - 2);
                    // Split by space, but handle quoted values properly
                    var spaceIndex = content.IndexOf(' ');
                    if (spaceIndex > 0)
                    {
                        var key = content.Substring(0, spaceIndex).Trim();
                        var value = content.Substring(spaceIndex + 1).Trim();
                        // Remove quotes if present
                        if (value.StartsWith("\"") && value.EndsWith("\""))
                        {
                            value = value.Substring(1, value.Length - 2);
                        }
                        // Only add non-empty values (skip empty strings like "")
                        if (!string.IsNullOrEmpty(value))
                        {
                            headers[key] = value;
                        }
                    }
                }
            }

            return headers;
        }
    }
}