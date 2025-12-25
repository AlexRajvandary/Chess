using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class Knight : PieceBase
    {
        public override PieceColor Color { get; set; }
        public override Position Position { get; set; }
        public override bool IsDead { get; set; }
        /// <summary>
        /// Direction for move
        /// </summary>
        private readonly Position[] Directions = new Position[] { 
            new Position(1, 2), new Position(2, 1), new Position(2, -1), new Position(1, -2), 
            new Position(-1, -2), new Position(-2, -1), new Position(-2, 1), new Position(-1, 2) };

        /*
         Чтобы узнать можно ли сходить на какую-то клетку, мы должны к текущему положению коня (к его координатам на поле) прибавить, например 2 вверх и 1 влево
         И мы получим какую-то клетку на поле. Затем нас интересует является ли данная клетка пустой: если да, то можем на нее сходить -> значит добавляем ее координаты в список AvailableMovesList
         Но для того, чтобы получить эту клетку из массива клеток, нужно убедиться что мы не выйдем за рамки массива
         для этого нужны условия ниже
         */
        /// <summary>
        /// условия для проверки хода/атаки в одном из 8-ми направлений
        /// </summary>
        private readonly Func<int, int, bool>[] Conditions = new Func<int, int, bool>[]
        {
            (x, y) => x < 7 && y < 6,
            (x, y) => x < 6 && y < 7,
            (x, y) => x < 6 && y > 0,
            (x, y) => x < 7 && y > 1,
            (x, y) => x > 0 && y > 1,
            (x, y) => x > 1 && y > 0,
            (x, y) => x > 1 && y < 7,
            (x, y) => x > 0 && y < 6
        };
        /// <summary>
        /// Gets list of available cells for move
        /// </summary>
        /// <param name="GameField">Game field</param>
        /// <returns>List of available cells for move</returns>
        public override List<Position> AvailableMoves(string[,] GameField)
        {
#if DEBUG
            TrackAvailableMoves();
#endif
            var AvailableMovesList = new List<Position>();

            for (int i = 0; i < 8; i++)
            {
                AvailableMoveInDirection(Directions[i], GameField, AvailableMovesList, Conditions[i]);
            }

            return AvailableMovesList;
        }

        private void AvailableMoveInDirection(Position Direction, string[,] GameField, List<Position> AvailableMovesList, Func<int, int, bool> Condition)
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

        public Knight(Position startPos, PieceColor color)
        {
            Position = startPos;
            Color = color;
            IsDead = false;
        }
        public override string ToString()
        {
            return Color == PieceColor.White ? "N" : "n"; ;
        }
        /// <summary>
        /// Вражеские фигуры
        /// </summary>
        private string pieces;
        /// <summary>
        /// Finds available enemy pieces for attack
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns></returns>
        public override List<Position> AvailableKills(string[,] GameField)
        {
            var result = new List<Position>();

            SetOppositeAndFriendPieces();

            for (int i = 0; i < 8; i++)
            {
                AvailablekillsInOneDirection(Directions[i], GameField, result, Conditions[i]);
            }

            return result;
        }

        private void SetOppositeAndFriendPieces()
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
        public override void ChangePosition(Position position)
        {
            this.Position = position;
        }

        /// <summary>
        /// Search for enemy pieces available for attack
        /// </summary>
        /// <param name="Direction">Direction</param>
        /// <param name="GameField">Game field</param>
        /// <param name="AvailableKillsList">List of coordinates of enemy pieces available for attack</param>
        /// <param name="Condition">Condition</param>
        private void AvailablekillsInOneDirection(Position Direction, string[,] GameField, List<Position> AvailableKillsList, Func<int, int, bool> Condition)
        {
            if (Condition(Position.X, Position.Y))
            {
                var attackPos = new Position(Position.X + Direction.X, Position.Y + Direction.Y);
                /*If the cell we're interested in is not empty AND has an enemy piece, add coordinates of this cell to list of pieces we can capture*/
                if (GameField[attackPos.X, attackPos.Y] != null && pieces.Contains(GameField[attackPos.X, attackPos.Y]))
                {
                    AvailableKillsList.Add(attackPos);
                }
            }
        }

        public override object Clone()
        {
            return new Knight(Position, Color);
        }
    }
}