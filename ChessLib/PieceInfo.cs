using ChessLib.Common;
using ChessLib.Pieces;

namespace ChessLib
{
    public class PieceInfo : IPieceInfo
    {
        public PieceType Type { get; }
        public PieceColor Color { get; }
        public Position Position { get; }

        public PieceInfo(PieceType type, PieceColor color, Position position)
        {
            Type = type;
            Color = color;
            Position = position;
        }

        public static PieceInfo FromPiece(IPiece piece)
        {
            var type = piece switch
            {
                King => PieceType.King,
                Queen => PieceType.Queen,
                Rook => PieceType.Rook,
                Bishop => PieceType.Bishop,
                Knight => PieceType.Knight,
                Pawn => PieceType.Pawn,
                _ => throw new System.ArgumentException($"Unknown piece type: {piece.GetType()}")
            };

            return new PieceInfo(type, piece.Color, piece.Position);
        }
    }
}

