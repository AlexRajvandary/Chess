using ChessLib.Common;
using System;
using System.Collections.Generic;

namespace ChessLib.Pieces
{
    public class Rook : PieceBase
    {
        /// <summary>
        /// обозначения вражеских фигур
        /// </summary>
        private string pieces;
        /// <summary>
        /// Обозначения своих фигур
        /// </summary>
        private string myPieces;

        public RookKind RookKind { get; }
        public bool IsMoved { get; set; }
        public override PieceColor Color { get; set; }
        public override Position Position { get; set; }
        public override bool IsDead { get; set; }

        /*Direction for piece search
         * (will add first coordinate to first coordinate of piece position
         *  and second to second in two-dimensional array (game field))
         */
        private readonly Position North = new Position(0, 1);
        private readonly Position South = new Position(0, -1);
        private readonly Position West = new Position(1, 0);
        private readonly Position East = new Position(-1, 0);

        //условия для поиска фигуры в массиве(игровом поле)
        private readonly Func<int, int, bool> NorthCondition = (int i, int j) => j < 8;
        private readonly Func<int, int, bool> SouthCondition = (int i, int j) => j > -1;
        private readonly Func<int, int, bool> WestCondition = (int i, int j) => i < 8;
        private readonly Func<int, int, bool> EastCondition = (int i, int j) => i > -1;

        /// <summary>
        /// Checks available moves for rook in 4 directions
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns>List of coordinates of available cells for move</returns>
        public override List<Position> AvailableMoves(string[,] GameField)
        {
#if DEBUG
            TrackAvailableMoves();
#endif
            var AvailableMovesList = new List<Position>();
            //North
            AvailableMovesInDirection(North, GameField, AvailableMovesList, NorthCondition);
            //South
            AvailableMovesInDirection(South, GameField, AvailableMovesList, SouthCondition);
            //West
            AvailableMovesInDirection(West, GameField, AvailableMovesList, WestCondition);
            //East
            AvailableMovesInDirection(East, GameField, AvailableMovesList, EastCondition);
            return AvailableMovesList;
        }
        /// <summary>
        /// Finds available cells for move
        /// </summary>
        /// <param name="Direction">Search direction</param>
        /// <param name="GameField">Game field</param>
        /// <param name="AvailableMovesList">List of available moves (newly found available cells are added here)</param>
        /// <param name="Condition">Condition for loop (depends on direction)</param>
        private void AvailableMovesInDirection(Position Direction, string[,] GameField, List<Position> AvailableMovesList, Func<int, int, bool> Condition)
        {
            for (int j = Position.Y + Direction.Y, i = Position.X + Direction.X; Condition(i, j); j += Direction.Y, i += Direction.X)
            {
                //if cell is not empty, cannot move there
                if (GameField[i, j] != " ")
                {
                    break;
                }
                AvailableMovesList.Add(new Position(i, j));
            }
        }

        public override string ToString()
        {
            return Color == PieceColor.White ? "R" : "r";
        }
        /// <summary>
        /// Finds enemy pieces for attack
        /// </summary>
        /// <param name="direction">Search direction (pair of numbers added to piece coordinates)</param>
        /// <param name="GameField">Game field</param>
        /// <param name="AvailableKillsList">List of enemy pieces for attack, where newly found enemy pieces are added</param>
        /// <param name="Condition">Condition for for loop (varies depending on search direction)</param>
        private void AvailableKillsInDirection(Position direction, string[,] GameField, List<Position> AvailableKillsList, Func<int, int, bool> Condition)
        {
            for (int i = Position.X + direction.X, j = Position.Y + direction.Y; Condition(i, j); i += direction.X, j += direction.Y)
            {
                //if hit own piece, cannot capture anyone
                if (myPieces.Contains(GameField[i, j]))
                {
                    break;
                }
                else if (pieces.Contains(GameField[i, j]))
                {
                    AvailableKillsList.Add(new Position(i, j));
                    break;
                }
            }
        }
        /// <summary>
        /// Search for enemy pieces for attack
        /// </summary>
        /// <param name="GameField">Game field</param>
        /// <returns>List of coordinates of enemy pieces for attack</returns>
        public override List<Position> AvailableKills(string[,] GameField)
        {
            var AvailableKillsList = new List<Position>();

            SetOppositeAndFreindlyPieces();

            //North
            AvailableKillsInDirection(North, GameField, AvailableKillsList, NorthCondition);
            //South
            AvailableKillsInDirection(South, GameField, AvailableKillsList, SouthCondition);
            //West
            AvailableKillsInDirection(West, GameField, AvailableKillsList, WestCondition);
            //East
            AvailableKillsInDirection(East, GameField, AvailableKillsList, EastCondition);
            return AvailableKillsList;
        }
        /// <summary>
        /// Устанавливает вражеские и свои фигуры
        /// </summary>
        private void SetOppositeAndFreindlyPieces()
        {
            if (Color == PieceColor.White)
            {
                pieces = "kbnpqr";
                myPieces = "KBNPQR";
            }
            else
            {
                pieces = "KBNPQR";
                myPieces = "kbnpqr";
            }
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