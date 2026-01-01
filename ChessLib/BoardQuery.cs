using ChessLib.Common;
using ChessLib.Pieces;
using ChessLib.Services;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib
{
    public class BoardQuery : IBoardQuery
    {
        private readonly List<IPiece> pieces;
        private readonly GameField gameField;
        private readonly string[,] gameFieldString;

        public BoardQuery(List<IPiece> pieces, GameField gameField, string[,] gameFieldString)
        {
            this.pieces = pieces;
            this.gameField = gameField;
            this.gameFieldString = gameFieldString;
        }

        public IPieceInfo GetPieceAt(Position position)
        {
            if (!position.IsValid())
                return null;

            var piece = pieces.FirstOrDefault(p => p.Position == position && !p.IsDead);
            return piece != null ? PieceInfo.FromPiece(piece) : null;
        }

        public bool IsCellFree(Position position)
        {
            if (!position.IsValid())
                return false;

            return GameField.IsCellFree(position, gameFieldString);
        }

        public bool IsCellAttacked(Position position, PieceColor byColor)
        {
            if (!position.IsValid())
                return false;

            var enemyPieces = pieces.Where(p => p.Color == byColor && !p.IsDead).ToList();
            return gameField.GetAtackStatus(enemyPieces, position, gameFieldString);
        }
    }
}

