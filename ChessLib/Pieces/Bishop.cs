using ChessLib.Common;
using System;
using System.Collections.Generic;

namespace ChessLib.Pieces
{
    public class Bishop : PieceBase
    {
        public override PieceColor Color { get; set; }
        public override Position Position { get; set; }
        public override bool IsDead { get; set; }

        public override string ToString()
        {
            return Color == PieceColor.White ? "B" : "b";
        }
        public override void ChangePosition(Position position)
        {
            this.Position = position;
        }

        public override object Clone()
        {
            return new Bishop(this.Position, this.Color);
        }

        public Bishop(Position startPosition, PieceColor color)
        {
            this.Color = color;
            Position = startPosition;
            IsDead = false;
        }
    }
}