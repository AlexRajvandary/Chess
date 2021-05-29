using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class King : IPiece
    {

        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool IsDead { get; set; }
        private void AvailableMoveInOneDirection(string[,] GameField, List<(int, int)> AvailableMovesList, (int, int) Direction, Func<int, int, bool> Condition)
        {
            if (Condition(Position.Item1, Position.Item2))
            {
                if (GameField[Position.Item1 + Direction.Item1, Position.Item2 + Direction.Item2] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 + Direction.Item1, Position.Item2 + Direction.Item2));
                }

            }
        }
        public List<(int, int)> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<(int, int)>();
            for (int i = 0; i < 8; i++)
            {
                AvailableMoveInOneDirection(GameField, AvailableMovesList, Directions[i], AttackConditions[i]);
            }



            return AvailableMovesList;

        }
        public King((int, int) Position, PieceColor color)
        {
            this.Position = Position;
            Color = color;
            IsDead = false;
        }
        public override string ToString()
        {
            return "k";
        }
        private string pieces;
        public List<(int, int)> AvailableKills(string[,] GameField)
        {
            var AvailableKillsList = new List<(int, int)>();

            GetOppositeAndFriendPieces();

            for (int i = 0; i < 8; i++)
            {
                if (AttackConditions[i](Position.Item1, Position.Item2))
                {
                    if (pieces.Contains(GameField[Position.Item1 + Directions[i].Item1, Position.Item2 + Directions[i].Item2]))
                    {
                        AvailableKillsList.Add((Position.Item1 + Directions[i].Item1, Position.Item2 + Directions[i].Item2));
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


        /// <summary>
        /// Условия для проверки доступных клеток для хода/атаки в 8-ми направлениях
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
        /// 8 направлений хода/атаки
        /// </summary>
        private readonly (int, int)[] Directions = new (int, int)[] {
            (1, 1)  ,
            (0, 1)  ,
            (-1, 1) ,
            (-1, 0) ,
            (1, 0)  ,
            (-1, -1),
            (0, -1) ,
            (1, -1)
        };
    }
}
