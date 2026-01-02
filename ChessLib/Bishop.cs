using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class Bishop : IPiece
    {
        public PieceColor Color { get; set; }
        public Position Position { get; set; }
        public bool IsDead { get; set; }

        //Direction for piece search (will add first coordinate to first coordinate of piece position and second to second) in two-dimensional array (game field)
        private readonly Position[] Directions = new Position[] { new Position(1, 1), new Position(-1, 1), new Position(1, -1), new Position(-1, -1) };

        //условия для поиска фигуры в массиве(игровом поле)
        private readonly Func<int, int, bool>[] Conditions = new Func<int, int, bool>[]
        {
            (int i, int j) => i < 8 & j < 8,//NorthEast
            (int i, int j) => i > -1 & j < 8,//NorthWest
            (int i, int j) => i < 8 & j > -1,//SouthEast
            (int i, int j) => i > -1 & j > -1//SouthWest
        };

        /// <summary>
        /// Checks available moves for bishop in four directions
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns>List of coordinates of free cells for move</returns>
        public List<Position> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<Position>();

            for (int i = 0; i < 4; i++)
            {
                AvailableMovesInDirection(Directions[i], GameField, AvailableMovesList, Conditions[i]);
            }

            return AvailableMovesList;
        }
        /// <summary>
        /// Checks available cells for move in specified direction
        /// </summary>
        /// <param name="Direction">Specified direction (pair of numbers that will be added to piece coordinates)</param>
        /// <param name="GameField">Game field</param>
        /// <param name="AvailableMovesList">List of available cells for move</param>
        /// <param name="Condition">Condition for for loop (varies depending on selected direction)</param>
        private void AvailableMovesInDirection(Position Direction, string[,] GameField, List<Position> AvailableMovesList, Func<int, int, bool> Condition)
        {
            for (int i = Position.X + Direction.X, j = Position.Y + Direction.Y; Condition(i, j); i += Direction.X, j += Direction.Y)
            {
                //if cell is not empty, cannot make move there
                if (GameField[i, j] != " ")
                {
                    break;
                }
                AvailableMovesList.Add(new Position(i, j));
            }
        }

        public override string ToString()
        {
            return Color == PieceColor.White ? "B" : "b";
        }

        /// <summary>
        /// Вражеские фигуры
        /// </summary>
        string pieces;
        /// <summary>
        /// Свои фигуры
        /// </summary>
        string myPieces;

        /// <summary>
        /// Search for enemy pieces that can be captured in one of four directions
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="GameField">Game field</param>
        /// <param name="AvailableKillsList">List of pieces that can be captured (newly found capturable piece is added here)</param>
        /// <param name="func">Conditions for direction of piece search in array (game field)</param>
        private void AvailableKillsInDirection(Position direction, string[,] GameField, List<Position> AvailableKillsList, Func<int, int, bool> func)
        {
            for (int i = Position.X + direction.X, j = Position.Y + direction.Y; func(i, j); i += direction.X, j += direction.Y)
            {
                //if hit own piece, cannot capture anyone
                if (myPieces.Contains(GameField[i, j]))
                {
                    break;
                }
                else if (pieces.Contains(GameField[i, j])) //if hit enemy piece, can capture it
                {
                    AvailableKillsList.Add(new Position(i, j));
                    break;
                }
            }
        }
        /// <summary>
        /// Search for enemy pieces that can be captured
        /// </summary>
        /// <param name="GameField">Game field</param>
        /// <returns>List of coordinates of enemy pieces for attack</returns>
        public List<Position> AvailableKills(string[,] GameField)
        {
            List<Position> AvailableKillsList = new List<Position>();
            SetOppositeAndFreindPieces();
            for (int i = 0; i < 4; i++)
            {
                AvailableKillsInDirection(Directions[i], GameField, AvailableKillsList, Conditions[i]);
            }

            return AvailableKillsList;
        }

        private void SetOppositeAndFreindPieces()
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
        public void ChangePosition(Position position)
        {
            this.Position = position;
        }

        public object Clone()
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
