using ChessLib.Common;
using System;
using System.Collections.Generic;

namespace ChessLib.Pieces
{
    public class Rook : PieceBase
    {
        public RookKind RookKind { get; }
        public bool IsMoved { get; set; }
        public override PieceColor Color { get; set; }
        public override Position Position { get; set; }
        public override bool IsDead { get; set; }

        public override string ToString()
        {
            return Color == PieceColor.White ? "R" : "r";
        }
        public override void ChangePosition(Position newPosition)
        {
            Position = newPosition;
        }

        public override object Clone()
        {
            return new Rook(Position, Color);
        }

        public Rook(Position position, PieceColor color)
        {
            this.Position = position;
            Color = color;
            IsDead = false;
            IsMoved = false;

            if (position.X == 0)
            {
                RookKind = RookKind.Queen;
            }
            else
            {
                RookKind = RookKind.Royal;
            }
        }
    }
}