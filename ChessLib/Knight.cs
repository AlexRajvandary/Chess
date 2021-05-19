using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class Knight : IPiece
    {
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool IsDead { get; set; }
        /// <summary>
        /// Получает список доступных для хода клеток 
        /// </summary>
        /// <param name="GameField">Игровое поле</param>
        /// <returns>Список доступных для хода клеток</returns>
        public List<(int, int)> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<(int, int)>();

            //Направления для хода
            var North = (1, 2);
            var Northeast = (2, 1);
            var East = (2, -1);
            var SouthEast = (1, -2);
            var South = (-1, -2);
            var SouthWest = (-2, -1);
            var West = (-2, 1);
            var NorthWest = (-1, 2);

            /*
             Чтобы узнать можно ли сходить на какую-то клетку, мы должны к текущему положению коня (к его координатам на поле) прибавить, например 2 вверх и 1 влево
             И мы получим какую-то клетку на поле. Затем нас интересует является ли данная клетка пустой: если да, то можем на нее сходить -> значит добавляем ее координаты в список AvailableMovesList
             Но для того, чтобы получить эту клетку из массива клеток, нужно убедиться что мы не выйдем за рамки массива
             для этого нужны условия ниже
             */
            Func<int, int, bool> NorthCondition = (x, y) => x < 7 && y < 6;
            Func<int, int, bool> NorthEastCondition = (x, y) => x < 6 && y < 7;
            Func<int, int, bool> EastCondition = (x, y) => x < 6 && y > 0;
            Func<int, int, bool> SouthEastCondition = (x, y) => x < 7 && y > 1;
            Func<int, int, bool> SouthCondition = (x, y) => x > 0 && y > 1;
            Func<int, int, bool> SouthWestCondition = (x, y) => x > 1 && y > 0;
            Func<int, int, bool> WestCondition = (x, y) => x > 1 && y < 7;
            Func<int, int, bool> NorthWestCondition = (x, y) => x > 0 && y < 6;


            AvailableMoveInDirection(North,GameField, AvailableMovesList,NorthCondition);
            AvailableMoveInDirection(Northeast, GameField, AvailableMovesList, NorthEastCondition);
            AvailableMoveInDirection(East, GameField, AvailableMovesList, EastCondition);
            AvailableMoveInDirection(SouthEast, GameField, AvailableMovesList, SouthEastCondition);
            AvailableMoveInDirection(South, GameField, AvailableMovesList, SouthCondition);
            AvailableMoveInDirection(SouthWest, GameField, AvailableMovesList, SouthWestCondition);
            AvailableMoveInDirection(West, GameField, AvailableMovesList, WestCondition);
            AvailableMoveInDirection(NorthWest, GameField, AvailableMovesList, NorthWestCondition);

            return AvailableMovesList;
        }

        private void AvailableMoveInDirection((int,int) Direction,string[,] GameField, List<(int, int)> AvailableMovesList, Func<int,int,bool> Condition)
        {
            if (Condition(Position.Item1,Position.Item2))
            {
                if (GameField[Position.Item1 + Direction.Item1, Position.Item2 + Direction.Item2] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 + Direction.Item1, Position.Item2 + Direction.Item2));
                }

            }
        }

        public Knight((int,int) startPos, PieceColor color)
        {
            Position = startPos;
            Color = color;
            IsDead = false;
        }
        public override string ToString()
        {
            return "n";
        }
        /// <summary>
        /// Вражеские фигуры
        /// </summary>
        private string pieces;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns></returns>
        public List<(int, int)> AvailableKills(string[,] GameField)
        {
            var result = new List<(int, int)>();

            if (Color == PieceColor.White)
            {
                pieces = "bnpqr";
            }
            else
            {
                pieces = "BNPQR";
            }

            //Направления для хода
            var North = (1, 2);
            var Northeast = (2, 1);
            var East = (2, -1);
            var SouthEast = (1, -2);
            var South = (-1, -2);
            var SouthWest = (-2, -1);
            var West = (-2, 1);
            var NorthWest = (-1, 2);

            /*
           Чтобы узнать можно ли сходить на какую-то клетку, мы должны к текущему положению коня (к его координатам на поле) прибавить, например 2 вверх и 1 влево
           И мы получим какую-то клетку на поле. Затем нас интересует является ли данная клетка пустой: если да, то можем на нее сходить -> значит добавляем ее координаты в список AvailableMovesList
           Но для того, чтобы получить эту клетку из массива клеток, нужно убедиться что мы не выйдем за рамки массива
           для этого нужны условия ниже
           */
            Func<int, int, bool> NorthCondition = (x, y) => x < 7 && y < 6;
            Func<int, int, bool> NorthEastCondition = (x, y) => x < 6 && y < 7;
            Func<int, int, bool> EastCondition = (x, y) => x < 6 && y > 0;
            Func<int, int, bool> SouthEastCondition = (x, y) => x < 7 && y > 1;
            Func<int, int, bool> SouthCondition = (x, y) => x > 0 && y > 1;
            Func<int, int, bool> SouthWestCondition = (x, y) => x > 1 && y > 0;
            Func<int, int, bool> WestCondition = (x, y) => x > 1 && y < 7;
            Func<int, int, bool> NorthWestCondition = (x, y) => x > 0 && y < 6;

            AvailablekillsInOneDirection(North,GameField, result,NorthCondition);
            AvailablekillsInOneDirection(Northeast, GameField, result, NorthEastCondition);
            AvailablekillsInOneDirection(East, GameField, result, EastCondition);
            AvailablekillsInOneDirection(SouthEast, GameField, result, SouthWestCondition);
            AvailablekillsInOneDirection(South, GameField, result, SouthCondition);
            AvailablekillsInOneDirection(SouthWest, GameField, result, SouthWestCondition);
            AvailablekillsInOneDirection(West, GameField, result, WestCondition);
            AvailablekillsInOneDirection(NorthWest, GameField, result, NorthWestCondition);

            return result;
        }
        /// <summary>
        /// Ищем вражеские фигуры, доступные для атаки
        /// </summary>
        /// <param name="Direction">Направление</param>
        /// <param name="GameField">Игровое поле</param>
        /// <param name="AvailableKillsList">список координат вражеских фигур, доступных для атаки</param>
        /// <param name="Condition">Условие</param>
        private void AvailablekillsInOneDirection((int,int) Direction,string[,] GameField, List<(int, int)> AvailableKillsList,Func<int,int,bool> Condition)
        {
            if (Condition(Position.Item1,Position.Item2))
            {
                /*Если интересующая нас клетка не пустая И на ней вражеская фигура, то добавляем координаты этой клетки в список фигур, которые мы можем съесть*/
                if (GameField[Position.Item1 + Direction.Item1, Position.Item2 + Direction.Item2] != null && pieces.Contains(GameField[Position.Item1 + Direction.Item1, Position.Item2 + Direction.Item2]))
                {
                    AvailableKillsList.Add((Position.Item1 + Direction.Item1, Position.Item2 + Direction.Item2));
                }

            }
        }
    }
}
