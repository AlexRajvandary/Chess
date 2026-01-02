using ChessLib.Common;
using System;
using System.Collections.Generic;

namespace ChessLib.Pieces
{
    public class Queen : PieceBase
    {
        public override PieceColor Color { get; set; }
        public override Position Position { get; set; }
        public override bool IsDead { get; set; }

        public override string ToString()
        {
            return Color == PieceColor.White ? "Q" : "q";
        }

        public override void ChangePosition(Position position)
        {
            this.Position = position;
        }

        public override object Clone()
        {
            return new Queen(Color, Position);
        }

        public Queen(PieceColor color, Position startPos)
        {
            Position = startPos;
            Color = color;
            IsDead = false;
        }
    }
}