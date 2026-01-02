using ChessLib.Common;
using ChessLib.Pieces;

namespace ChessLib
{
    public interface IBoardRepresentation
    {
        IPieceInfo GetPieceAt(Position position);
        void SetPieceAt(Position position, IPieceInfo piece);
        void ClearCell(Position position);
        bool IsCellFree(Position position);
        void InitializeFromPieces(System.Collections.Generic.List<IPiece> pieces);
    }
}

