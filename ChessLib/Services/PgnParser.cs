using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChessLib.Services
{
    public static class PgnParser
    {
        public static List<string> ParseMoves(string pgn)
        {
            var moves = new List<string>();
            var lines = pgn.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var inMovesSection = false;
            var movesText = new StringBuilder();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    continue;
                }

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

            movesString = Regex.Replace(movesString, @"\s+(1-0|0-1|1/2-1/2|\*)\s*$", "");
            movesString = Regex.Replace(movesString, @"\s+", " ");

            var movePattern = @"(\d+)\.\s*([^\d]+?)(?=\s*\d+\.|$)";
            var matches = Regex.Matches(movesString, movePattern);
            
            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    var movePair = match.Groups[2].Value.Trim();
                    var individualMoves = movePair.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var move in individualMoves)
                    {
                        var cleanMove = move.Trim();
                        
                        if (Regex.IsMatch(cleanMove, @"^\d+\.?$"))
                        {
                            continue;
                        }
                        
                        var numberDotMatch = Regex.Match(cleanMove, @"^(\d+)\.(.+)$");
                        if (numberDotMatch.Success && numberDotMatch.Groups.Count >= 3)
                        {
                            cleanMove = numberDotMatch.Groups[2].Value.Trim();
                        }
                        
                        if (cleanMove == "1-0" || cleanMove == "0-1" || cleanMove == "1/2-1/2" || cleanMove == "*")
                        {
                            continue;
                        }
                        
                        cleanMove = cleanMove.TrimEnd('+', '#');
                        
                        if (!string.IsNullOrEmpty(cleanMove))
                        {
                            moves.Add(cleanMove);
                        }
                    }
                }
            }

            if (moves.Count == 0)
            {
                var parts = movesString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    
                    if (Regex.IsMatch(trimmed, @"^\d+\.?$"))
                    {
                        continue;
                    }
                    
                    var numberDotMatch = Regex.Match(trimmed, @"^(\d+)\.(.+)$");
                    if (numberDotMatch.Success && numberDotMatch.Groups.Count >= 3)
                    {
                        trimmed = numberDotMatch.Groups[2].Value.Trim();
                    }
                    
                    if (trimmed == "1-0" || trimmed == "0-1" || trimmed == "1/2-1/2" || trimmed == "*")
                    {
                        continue;
                    }
                    
                    trimmed = trimmed.TrimEnd('+', '#');
                    
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        moves.Add(trimmed);
                    }
                }
            }

            return moves;
        }

        public static Dictionary<string, string> ParseHeaders(string pgn)
        {
            var headers = new Dictionary<string, string>();
            var lines = pgn.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    var content = trimmed.Substring(1, trimmed.Length - 2);
                    var spaceIndex = content.IndexOf(' ');
                    if (spaceIndex > 0)
                    {
                        var key = content.Substring(0, spaceIndex).Trim();
                        var value = content.Substring(spaceIndex + 1).Trim();
                        if (value.StartsWith("\"") && value.EndsWith("\""))
                        {
                            value = value.Substring(1, value.Length - 2);
                        }
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

