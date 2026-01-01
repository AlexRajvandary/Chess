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
        private readonly IBoardRepresentation boardRepresentation;
        private readonly string[,] gameFieldString;

        public BoardQuery(List<IPiece> pieces, GameField gameField, string[,] gameFieldString)
        {
            this.pieces = pieces;
            this.gameField = gameField;
            this.gameFieldString = gameFieldString;
            this.boardRepresentation = null;
        }

        public BoardQuery(List<IPiece> pieces, GameField gameField, IBoardRepresentation boardRepresentation)
        {
            this.pieces = pieces;
            this.gameField = gameField;
            this.boardRepresentation = boardRepresentation;
            this.gameFieldString = null;
        }

        public IPieceInfo GetPieceAt(Position position)
        {
            if (!position.IsValid())
                return null;

            if (boardRepresentation != null)
            {
                return boardRepresentation.GetPieceAt(position);
            }

            var piece = pieces.FirstOrDefault(p => p.Position == position && !p.IsDead);
            return piece != null ? PieceInfo.FromPiece(piece) : null;
        }

        public bool IsCellFree(Position position)
        {
            if (!position.IsValid())
                return false;

            if (boardRepresentation != null)
            {
                return boardRepresentation.IsCellFree(position);
            }

            return GameField.IsCellFree(position, gameFieldString);
        }

        public bool IsCellAttacked(Position position, PieceColor byColor)
        {
            if (!position.IsValid())
                return false;

            var enemyPieces = pieces.Where(p => p.Color == byColor && !p.IsDead).ToList();
            
            if (boardRepresentation != null && gameFieldString == null)
            {
                var tempBoard = new Representations.ArrayBoardRepresentation();
                tempBoard.InitializeFromPieces(pieces);
                var tempString = ((Representations.ArrayBoardRepresentation)tempBoard).GetStringArray();
                return gameField.GetAtackStatus(enemyPieces, position, tempString);
            }

            return gameField.GetAtackStatus(enemyPieces, position, gameFieldString);
        }
    }
}

