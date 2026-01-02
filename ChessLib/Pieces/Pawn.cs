using ChessLib.Common;
using System;
using System.Collections.Generic;

namespace ChessLib.Pieces
{
    public class Pawn : PieceBase
    {
        public override bool IsDead { get; set; }
        public override PieceColor Color { get; set; }
        public override Position Position { get; set; }

        /// <summary>
        /// Starting position, needed to check available moves for pawn (if pawn is in starting position, there are 2 move options)
        /// </summary>
        public Position StartPos;
        private List<Position> EnemyMoveDir { get; set; }

        public bool EnPassantAvailable { get; set; }

        public override string ToString()
        {
            return Color == PieceColor.White ? "P" : "p"; ;
        }


        public bool AvailableEnPassent(Pawn EnemyPawn)
        {
            var enemyTargetPos = new Position(EnemyPawn.StartPos.X + EnemyMoveDir[1].X, EnemyPawn.StartPos.Y + EnemyMoveDir[1].Y);
            bool IsEnemyPositionCorrect = EnemyPawn.Position == enemyTargetPos;
            bool IsVerticalPositionCorrect = Position.Y == EnemyPawn.Position.Y;
            bool IsHorizontalposition1Correct = Position.X == EnemyPawn.Position.X + 1;
            bool IsHorizontalposition2Correct = Position.X == EnemyPawn.Position.X - 1;
            return EnemyPawn.EnPassantAvailable && IsEnemyPositionCorrect && (IsHorizontalposition1Correct || IsHorizontalposition2Correct) && IsVerticalPositionCorrect;
        }

        public override void ChangePosition(Position position)
        {
            this.Position = position;
        }

        public override object Clone()
        {
            return new Pawn(Color, Position);
        }

        public Pawn(PieceColor color, Position position)
        {
            Color = color;
            if (Color == PieceColor.White)
            {
                EnemyMoveDir = new List<Position> { new Position(0, -1), new Position(0, -2) };
            }
            else
            {
                EnemyMoveDir = new List<Position> { new Position(0, 1), new Position(0, 2) };
            }
            StartPos = position;
            Position = StartPos;
            IsDead = false;
        }
    }
}