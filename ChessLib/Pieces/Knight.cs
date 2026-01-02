using ChessLib.Common;
using System;
using System.Collections.Generic;

namespace ChessLib.Pieces
{
    public class Knight : PieceBase
    {
        public override PieceColor Color { get; set; }
        public override Position Position { get; set; }
        public override bool IsDead { get; set; }

        /*
         Чтобы узнать можно ли сходить на какую-то клетку, мы должны к текущему положению коня (к его координатам на поле) прибавить, например 2 вверх и 1 влево
         И мы получим какую-то клетку на поле. Затем нас интересует является ли данная клетка пустой: если да, то можем на нее сходить -> значит добавляем ее координаты в список AvailableMovesList
         Но для того, чтобы получить эту клетку из массива клеток, нужно убедиться что мы не выйдем за рамки массива
         для этого нужны условия ниже
         */
        public Knight(Position startPos, PieceColor color)
        {
            Position = startPos;
            Color = color;
            IsDead = false;
        }
        public override string ToString()
        {
            return Color == PieceColor.White ? "N" : "n";
        }

        public override void ChangePosition(Position position)
        {
            this.Position = position;
        }

        public override object Clone()
        {
            return new Knight(Position, Color);
        }
    }
}