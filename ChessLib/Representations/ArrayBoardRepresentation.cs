using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib.Representations
{
    public class ArrayBoardRepresentation : IBoardRepresentation
    {
        private readonly string[,] board;

        public ArrayBoardRepresentation()
        {
            board = new string[8, 8];
            Clear();
        }

        public IPieceInfo GetPieceAt(Position position)
        {
            if (!position.IsValid())
                return null;

            var pieceString = board[position.X, position.Y];
            if (string.IsNullOrWhiteSpace(pieceString) || pieceString == " ")
                return null;

            return ParsePieceString(pieceString, position);
        }

        public void SetPieceAt(Position position, IPieceInfo piece)
        {
            if (!position.IsValid())
                return;

            if (piece == null)
            {
                board[position.X, position.Y] = " ";
            }
            else
            {
                board[position.X, position.Y] = GetPieceString(piece);
            }
        }

        public void ClearCell(Position position)
        {
            if (position.IsValid())
            {
                board[position.X, position.Y] = " ";
            }
        }

        public bool IsCellFree(Position position)
        {
            if (!position.IsValid())
                return false;

            var pieceString = board[position.X, position.Y];
            return string.IsNullOrWhiteSpace(pieceString) || pieceString == " ";
        }

        public void InitializeFromPieces(List<IPiece> pieces)
        {
            Clear();

            foreach (var piece in pieces.Where(p => !p.IsDead))
            {
                var pieceString = piece.Color == PieceColor.White
                    ? piece.ToString().ToUpper()
                    : piece.ToString();
                board[piece.Position.X, piece.Position.Y] = pieceString;
            }
        }

        public string[,] GetStringArray()
        {
            return board;
        }

        private void Clear()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = " ";
                }
            }
        }

        private static string GetPieceString(IPieceInfo piece)
        {
            var pieceChar = piece.Type switch
            {
                PieceType.Pawn => 'p',
                PieceType.Rook => 'r',
                PieceType.Knight => 'n',
                PieceType.Bishop => 'b',
                PieceType.Queen => 'q',
                PieceType.King => 'k',
                _ => ' '
            };

            return piece.Color == PieceColor.White
                ? char.ToUpper(pieceChar).ToString()
                : pieceChar.ToString();
        }

        private static IPieceInfo ParsePieceString(string pieceString, Position position)
        {
            if (string.IsNullOrWhiteSpace(pieceString) || pieceString == " ")
                return null;

            var pieceChar = char.ToLower(pieceString[0]);
            var isWhite = char.IsUpper(pieceString[0]);

            var pieceType = pieceChar switch
            {
                'p' => PieceType.Pawn,
                'r' => PieceType.Rook,
                'n' => PieceType.Knight,
                'b' => PieceType.Bishop,
                'q' => PieceType.Queen,
                'k' => PieceType.King,
                _ => (PieceType?)null
            };

            if (pieceType == null)
                return null;

            var color = isWhite ? PieceColor.White : PieceColor.Black;
            return new PieceInfo(pieceType.Value, color, position);
        }
    }
}

