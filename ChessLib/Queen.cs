using System;
using System.Collections.Generic;

namespace ChessLib
{
    public class Queen : IPiece
    {
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool IsDead { get; set; }

        /// <summary>
        /// Проверка доступных ходов для слона в четырех направлениях
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns>Список координат свободных для хода клеток</returns>
        public List<(int, int)> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<(int, int)>();
            //направление поиска фигуры (будет прибавлять первую координату к первой координате позиции фигуры и вторую ко второй) по двумерному массиву (игрового поля)
            (int, int) NorthEast = (1, 1);
            (int, int) NorthWest = (-1, 1);
            (int, int) SouthEast = (1, -1);
            (int, int) SouthWest = (-1, -1);
            (int, int) North = (0, 1);
            (int, int) South = (0, -1);
            (int, int) West = (1, 0);
            (int, int) East = (-1, 0);


            //условия для поиска фигуры в массиве(игровом поле)
            Func<int, int, bool> NorthEastCondition = (int i, int j) => i < 8 & j < 8;
            Func<int, int, bool> NorthWestCondition = (int i, int j) => i > -1 & j < 8;
            Func<int, int, bool> SouthEastCondition = (int i, int j) => i < 8 & j > -1;
            Func<int, int, bool> SouthWestCondition = (int i, int j) => i > -1 & j > -1;
            Func<int, int, bool> NorthCondition = (int i, int j) => j < 8;
            Func<int, int, bool> SouthCondition = (int i, int j) => j > -1;
            Func<int, int, bool> WestCondition = (int i, int j) => i < 8;
            Func<int, int, bool> EastCondition = (int i, int j) => i > -1;
           
            //Север
            AvailableMovesInDirection(North, GameField, AvailableMovesList, NorthCondition);
            //Юг
            AvailableMovesInDirection(South, GameField, AvailableMovesList, SouthCondition);
            //Запад
            AvailableMovesInDirection(West, GameField, AvailableMovesList, WestCondition);
            //восток
            AvailableMovesInDirection(East, GameField, AvailableMovesList, EastCondition);
            //Северо-восток
            AvailableMovesInDirection(NorthEast, GameField, AvailableMovesList, NorthEastCondition);
            //северо-запад
            AvailableMovesInDirection(NorthWest, GameField, AvailableMovesList, NorthWestCondition);
            //Юго-Восток
            AvailableMovesInDirection(SouthEast, GameField, AvailableMovesList, SouthEastCondition);
            //Юго-Запад
            AvailableMovesInDirection(SouthWest, GameField, AvailableMovesList, SouthWestCondition);
            return AvailableMovesList;
        }
        /// <summary>
        /// Проверяет доступные клетки для хода в определенном направлении
        /// </summary>
        /// <param name="Direction">Заданное направление (пара чисел, которая будет прибавляться к координатам фигуры)</param>
        /// <param name="GameField">Игровое поле</param>
        /// <param name="AvailableMovesList">Список доступных для хода клеток</param>
        /// <param name="Condition">условие для цикла for (разное в зависимости от выбранного направления)</param>
        private void AvailableMovesInDirection((int, int) Direction, string[,] GameField, List<(int, int)> AvailableMovesList, Func<int, int, bool> Condition)
        {
            for (int i = Position.Item1 + Direction.Item1, j = Position.Item2 + Direction.Item2; Condition(i, j); i += Direction.Item1, j += Direction.Item2)
            {
                //если клетка не пустая, то сделать на нее ход нельзя
                if (GameField[i, j] != " ")
                {
                    break;
                }
                AvailableMovesList.Add((i, j));
            }
        }

        public override string ToString()
        {
            return "q";
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
        /// Поиск вражеских фигур, которые можно съесть в одном из четырех направлений
        /// </summary>
        /// <param name="direction">направление</param>
        /// <param name="GameField">игровое поле</param>
        /// <param name="AvailableKillsList">список фигур, которые можно съесть (сюда добавляем вновь найденную фигуру, которую можно съесть)</param>
        /// <param name="func">условия для направления поиска фигур в массиве(игровом поле)</param>
        private void AvailableKillsInDirection((int, int) direction, string[,] GameField, List<(int, int)> AvailableKillsList, Func<int, int, bool> func)
        {
            for (int i = Position.Item1 + direction.Item1, j = Position.Item2 + direction.Item2; func(i, j); i += direction.Item1, j += direction.Item2)
            {
                ///если уперлись в свою фигуру, то съесть никого нельзя
                if (myPieces.Contains(GameField[i, j]))
                {
                    break;
                }
                else//если уперлись во вражескую фигуру, то можем ее съесть
                      if (pieces.Contains(GameField[i, j]))
                {

                    AvailableKillsList.Add((i, j));
                    break;
                }
            }
        }
        /// <summary>
        /// Поиск вражеских фигур, которые можно съесть
        /// </summary>
        /// <param name="GameField">Игровое поле</param>
        /// <returns>Список координат вражеских фигур для атаки</returns>
        public List<(int, int)> AvailableKills(string[,] GameField)
        {
            List<(int, int)> AvailableKillsList = new List<(int, int)>();

            if (Color == PieceColor.White)
            {
                pieces = "bnpqr";
                myPieces = "BNPQR";
            }
            else
            {
                pieces = "BNPQR";
                myPieces = "bnpqr";
            }
            //направление поиска фигуры (будет прибавлять первую координату к первой координате позиции фигуры и вторую ко второй) по двумерному массиву (игрового поля)
            (int, int) NorthEast = (1, 1);
            (int, int) Northwest = (-1, 1);
            (int, int) SouthEast = (1, -1);
            (int, int) SouthWest = (-1, -1);
            (int, int) North = (0, 1);
            (int, int) South = (0, -1);
            (int, int) West = (1, 0);
            (int, int) East = (-1, 0);


            //условия для поиска фигуры в массиве(игровом поле)
            Func<int, int, bool> NorthEastCondition = (int i, int j) => i < 8 & j < 8;
            Func<int, int, bool> NorthWestCondition = (int i, int j) => i > -1 & j < 8;
            Func<int, int, bool> SouthEastCondition = (int i, int j) => i < 8 & j > -1;
            Func<int, int, bool> SouthWestCondition = (int i, int j) => i > -1 & j > -1;
            Func<int, int, bool> NorthCondition = (int i, int j) => j < 8;
            Func<int, int, bool> SouthCondition = (int i, int j) => j > -1;
            Func<int, int, bool> WestCondition = (int i, int j) => i < 8;
            Func<int, int, bool> EastCondition = (int i, int j) => i > -1;

            //Северо-восток
            AvailableKillsInDirection(NorthEast, GameField, AvailableKillsList, NorthEastCondition);
            //северо-запад
            AvailableKillsInDirection(Northwest, GameField, AvailableKillsList, NorthWestCondition);
            //Юго-Восток
            AvailableKillsInDirection(SouthEast, GameField, AvailableKillsList, SouthEastCondition);
            //Юго-Запад
            AvailableKillsInDirection(SouthWest, GameField, AvailableKillsList, SouthWestCondition);
            //Север
            AvailableKillsInDirection(North, GameField, AvailableKillsList, NorthCondition);
            //Юг
            AvailableKillsInDirection(South, GameField, AvailableKillsList, SouthCondition);
            //Запад
            AvailableKillsInDirection(West, GameField, AvailableKillsList, WestCondition);
            //восток
            AvailableKillsInDirection(East, GameField, AvailableKillsList, EastCondition);

            return AvailableKillsList;

        }


        public Queen(PieceColor color, (int,int) startPos)
        {
            Position = startPos;
            Color = color;
            IsDead = false;
        }
    }
}
