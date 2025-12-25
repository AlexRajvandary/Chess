using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class Queen : PieceBase
    {
        public override PieceColor Color { get; set; }
        public override Position Position { get; set; }
        public override bool IsDead { get; set; }

        /// <summary>
        /// Conditions for checking
        /// </summary>
        private readonly Func<int, int, bool>[] Conditions = {
            (int i, int j) => i < 8 & j < 8,
            (int i, int j) => i > -1 & j < 8,
            (int i, int j) => i < 8 & j > -1,
            (int i, int j) => i > -1 & j > -1,
            (int i, int j) => j < 8,
            (int i, int j) => j > -1,
            (int i, int j) => i < 8,
            (int i, int j) => i > -1};
        /// <summary>
        /// Directions for checking
        /// </summary>
        private readonly Position[] Directions = new Position[] { 
            new Position(1, 1), new Position(-1, 1), new Position(1, -1), new Position(-1, -1), 
            new Position(0, 1), new Position(0, -1), new Position(1, 0), new Position(-1, 0) };

        /// <summary>
        /// Checks available moves for queen in 8 directions
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns>List of coordinates of free cells for move</returns>
        public override List<Position> AvailableMoves(string[,] GameField)
        {
#if DEBUG
            TrackAvailableMoves();
#endif
            var AvailableMovesList = new List<Position>();
            for (int i = 0; i < 8; i++)
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
            return Color == PieceColor.White ? "Q" : "q";
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
        /// Search for enemy pieces that can be captured in one of directions
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
        public override List<Position> AvailableKills(string[,] GameField)
        {
            List<Position> AvailableKillsList = new List<Position>();
            SetOppositeAndFreindPieces();
            for (int i = 0; i < 8; i++)
            {
                AvailableKillsInDirection(Directions[i], GameField, AvailableKillsList, Conditions[i]);
            }

            return AvailableKillsList;
        }
        /// <summary>
        /// Устанавливает свои и вражеские фигуры
        /// </summary>
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