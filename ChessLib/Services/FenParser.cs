using ChessLib.Common;
using ChessLib.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib.Services
{
    public static class FenParser
    {
        private static readonly Dictionary<char, PieceType> PieceTypeMap = new Dictionary<char, PieceType>
        {
            {'p', PieceType.Pawn}, {'P', PieceType.Pawn},
            {'r', PieceType.Rook}, {'R', PieceType.Rook},
            {'n', PieceType.Knight}, {'N', PieceType.Knight},
            {'b', PieceType.Bishop}, {'B', PieceType.Bishop},
            {'q', PieceType.Queen}, {'Q', PieceType.Queen},
            {'k', PieceType.King}, {'K', PieceType.King}
        };

        public static GameStateFromFen ParseFen(string fen)
        {
            if (string.IsNullOrWhiteSpace(fen))
                throw new ArgumentException("FEN string cannot be empty", nameof(fen));

            var parts = fen.Trim().Split(' ');
            if (parts.Length < 4)
                throw new ArgumentException("Invalid FEN format", nameof(fen));

            var pieces = ParsePiecePlacement(parts[0]);
            var activeColor = ParseActiveColor(parts[1]);
            var castlingRights = parts.Length > 2 ? parts[2] : "-";
            var enPassant = parts.Length > 3 ? parts[3] : "-";

            return new GameStateFromFen
            {
                Pieces = pieces,
                ActiveColor = activeColor,
                CastlingRights = castlingRights,
                EnPassant = enPassant
            };
        }

        private static List<IPiece> ParsePiecePlacement(string placement)
        {
            var pieces = new List<IPiece>();
            var ranks = placement.Split('/');
            
            if (ranks.Length != 8)
                throw new ArgumentException("Invalid piece placement: must have 8 ranks", nameof(placement));

            for (int rankIndex = 0; rankIndex < 8; rankIndex++)
            {
                int file = 0;
                var rank = ranks[7 - rankIndex];

                foreach (var ch in rank)
                {
                    if (char.IsDigit(ch))
                    {
                        file += int.Parse(ch.ToString());
                    }
                    else if (PieceTypeMap.ContainsKey(ch))
                    {
                        var pieceType = PieceTypeMap[ch];
                        var color = char.IsUpper(ch) ? PieceColor.White : PieceColor.Black;
                        var position = new Position(file, rankIndex);

                        var piece = CreatePiece(pieceType, color, position);
                        pieces.Add(piece);
                        file++;
                    }
                }
            }

            return pieces;
        }

        private static PieceColor ParseActiveColor(string colorStr)
        {
            return colorStr.ToLower() == "w" ? PieceColor.White : PieceColor.Black;
        }

        private static IPiece CreatePiece(PieceType type, PieceColor color, Position position)
        {
            return type switch
            {
                PieceType.Pawn => new Pawn(color, position),
                PieceType.Rook => new Rook(position, color),
                PieceType.Knight => new Knight(position, color),
                PieceType.Bishop => new Bishop(position, color),
                PieceType.Queen => new Queen(color, position),
                PieceType.King => new King(position, color),
                _ => throw new ArgumentException($"Unknown piece type: {type}")
            };
        }
    }

    public class GameStateFromFen
    {
        public List<IPiece> Pieces { get; set; }
        public PieceColor ActiveColor { get; set; }
        public string CastlingRights { get; set; }
        public string EnPassant { get; set; }
    }
}

