using ChessLib.Common;
using ChessLib.Services;
using System;
using System.Collections.Generic;

namespace ChessLib.Pieces
{
    public class King : PieceBase
    {
        public King(Position position, PieceColor color)
        {
            this.Position = position;
            Color = color;
            IsDead = false;
            IsMoved = false;
        }

        public override PieceColor Color { get; set; }
        public override Position Position { get; set; }
        public override bool IsDead { get; set; }
        public bool IsMoved { get; set; }

        public bool ShortCastling(Rook rook, GameField gameField, List<IPiece> pieces, string[,] gameFieldStr)
        {
            if (!IsMoved && !rook.IsMoved)
            {
                bool isAttacked = false;
                bool isFree = true;
                for (int i = Position.X + 1; i < 7; i++)
                {
                    var checkPos = new Position(i, Position.Y);
                    if (gameField.GetAtackStatus(pieces, checkPos, gameFieldStr))
                    {
                        isAttacked = true;
                    }

                    if (!GameField.IsCellFree(checkPos, gameFieldStr))
                    {
                        isFree = false;
                    }
                }

                return !isAttacked && isFree;
            }

            return false;
        }
       
        public bool LongCastling(Rook rook, GameField gameField, List<IPiece> EnemyPieces, string[,] gameFieldStr)
        {
            if (!IsMoved && !rook.IsMoved)
            {
                bool isAttacked = false;
                bool isFree = true;
                for (int i = Position.X - 1; i > 0; i--)
                {
                    var checkPos = new Position(i, Position.Y);
                    if (gameField.GetAtackStatus(EnemyPieces, checkPos, gameFieldStr))
                    {
                        isAttacked = true;
                    }

                    if (!GameField.IsCellFree(checkPos, gameFieldStr))
                    {
                        isFree = false;
                    }
                }

                return !isAttacked && isFree;
            }

            return false;
        }
      
        public override string ToString()
        {
            return Color == PieceColor.White ? "K" : "k";
        }

        public override void ChangePosition(Position newPosition)
        {
            Position = newPosition;
        }

        public override object Clone()
        {
            return new King(this.Position, this.Color);
        }
    }
}