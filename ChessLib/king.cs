using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class King : IPiece
    {
        private string pieces;

        public King(Position position, PieceColor color)
        {
            this.Position = position;
            Color = color;
            IsDead = false;
            IsMoved = false;
        }

        public PieceColor Color { get; set; }
        public Position Position { get; set; }
        public bool IsDead { get; set; }
        public bool IsMoved { get; set; }

        private void AvailableMoveInOneDirection(string[,] GameField, List<Position> AvailableMovesList, Position Direction, Func<int, int, bool> Condition)
        {
            if (Condition(Position.X, Position.Y))
            {
                var newPos = new Position(Position.X + Direction.X, Position.Y + Direction.Y);
                if (GameField[newPos.X, newPos.Y] == " ")
                {
                    AvailableMovesList.Add(newPos);
                }
            }
        }

        public List<Position> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<Position>();
            for (int i = 0; i < 8; i++)
            {
                AvailableMoveInOneDirection(GameField, AvailableMovesList, Directions[i], AttackConditions[i]);
            }

            return AvailableMovesList;
        }
      
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

                    if (!gameField.IsCellFree(checkPos, gameFieldStr))
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

                    if (!gameField.IsCellFree(checkPos, gameFieldStr))
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
        
        public List<Position> AvailableKills(string[,] GameField)
        {
            var AvailableKillsList = new List<Position>();

            GetOppositeAndFriendPieces();

            for (int i = 0; i < 8; i++)
            {
                if (AttackConditions[i](Position.X, Position.Y))
                {
                    var attackPos = new Position(Position.X + Directions[i].X, Position.Y + Directions[i].Y);
                    if (pieces.Contains(GameField[attackPos.X, attackPos.Y]))
                    {
                        AvailableKillsList.Add(attackPos);
                    }
                }
            }

            return AvailableKillsList;
        }

        private void GetOppositeAndFriendPieces()
        {
            if (Color == PieceColor.White)
            {
                pieces = "kbnpqr";
            }
            else
            {
                pieces = "KBNPQR";
            }
        }

        public void ChangePosition(Position newPosition)
        {
            Position = newPosition;
        }

        public object Clone()
        {
            return new King(this.Position, this.Color);
        }

        /// <summary>
        /// Conditions for checking available cells for move/attack in 8 directions
        /// </summary>
        private readonly Func<int, int, bool>[] AttackConditions = new Func<int, int, bool>[] {
            (int x, int y) => x < 7 && y < 7,
            (int x, int y) => y < 7,
            (int x, int y) => x > 0 && y < 7,
            (int x, int y) => x > 0,
            (int x, int y) => x < 7,
            (int x, int y) => x > 0 && y > 0,
            (int x, int y) => y > 0,
            (int x, int y) => x < 7 && y > 0
         };
        /// <summary>
        /// 8 directions for move/attack
        /// </summary>
        private readonly Position[] Directions = new Position[] {
            new(1, 1),
            new(0, 1),
            new(-1, 1),
            new(-1, 0),
            new(1, 0),
            new(-1, -1),
            new(0, -1),
            new(1, -1)
        };
    }
}