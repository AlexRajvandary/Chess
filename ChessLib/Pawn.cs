using System;
using System.Collections.Generic;

namespace ChessLib
{

    public class Pawn : IPiece
    {
        public bool IsDead { get; set; }
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }

        /// <summary>
        /// Стартовая позиция, нужна для проверки доступных ходов у пешки (если пешка в начальной позиции, то существует 2 варианта хода)
        /// </summary>
        public (int, int) StartPos;
        private Func<(int, int), bool> EndVerticalPos;
        private readonly (int, int)[] DirectionsForMove = new (int, int)[] { (0, 1), (0, -1) };

        public List<(int, int)> MoveDir { get; set; }
        private List<(int, int)> EnemyMoveDir { get; set; }
        /// <summary>
        /// проверяет все доступные ходы для текущей пешки
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns></returns>
        public List<(int, int)> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<(int, int)>();

            if (this.Position == this.StartPos)
            {
                if (GameField[Position.Item1 + MoveDir[0].Item1, Position.Item2 + MoveDir[0].Item2] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 + MoveDir[0].Item1, Position.Item2 + MoveDir[0].Item2));
                    if (GameField[Position.Item1 + MoveDir[1].Item1, Position.Item2 + MoveDir[1].Item2] == " ")
                    {
                        AvailableMovesList.Add((Position.Item1 + MoveDir[1].Item1, Position.Item2 + MoveDir[1].Item2));
                    }
                }

            }
            else
            {
                if (EndVerticalPos(Position) && GameField[Position.Item1 + MoveDir[0].Item1, Position.Item2 + MoveDir[0].Item2] == " ")
                {
                    AvailableMovesList.Add((Position.Item1 + MoveDir[0].Item1, Position.Item2 + MoveDir[0].Item2));
                }
            }
            return AvailableMovesList;
        }
        public bool EnPassantAvailable { get; set; }

        public override string ToString()
        {
            return "p";
        }

        /// <summary>
        /// Направления для атаки
        /// </summary>
        private List<(int, int)> AttackDir { get; set; }


        /// <summary>
        /// Условия для атаки
        /// </summary>
        private Func<int, int, bool>[] Conditions { get; set; }

        /// <summary>
        /// Ищет доступные для атаки вражеские фигуры
        /// </summary>
        /// <param name="GameField">Игровое поле</param>
        /// <returns>Возвращает список координат доступных для атаки фигур</returns>
        public List<(int, int)> AvailableKills(string[,] GameField)
        {
            var AvailableKillsList = new List<(int, int)>();

            GetOppositeAndFriendPieces();
            for (int i = 0; i < 2; i++)
            {

                if (Conditions[i](Position.Item1, Position.Item2))
                {

                    if (GameField[Position.Item1 + AttackDir[i].Item1, Position.Item2 + AttackDir[i].Item2] != " " && pieces.Contains(GameField[Position.Item1 + AttackDir[i].Item1, Position.Item2 + AttackDir[i].Item2]))
                    {
                        AvailableKillsList.Add((Position.Item1 + AttackDir[i].Item1, Position.Item2 + AttackDir[i].Item2));
                    }
                }

            }

            return AvailableKillsList;

        }

        public List<(int, int)> AvailableKills(string[,] GameField, Pawn EnemyPawn)
        {
            var AvailableKillsList = new List<(int, int)>();

            GetOppositeAndFriendPieces();
            if (EnemyPawn is null)
            {
                AvailableKillsList = AvailableKills(GameField);
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {

                    if (Conditions[i](Position.Item1, Position.Item2))
                    {

                        if (GameField[Position.Item1 + AttackDir[i].Item1, Position.Item2 + AttackDir[i].Item2] != " " && pieces.Contains(GameField[Position.Item1 + AttackDir[i].Item1, Position.Item2 + AttackDir[i].Item2]))
                        {
                            AvailableKillsList.Add((Position.Item1 + AttackDir[i].Item1, Position.Item2 + AttackDir[i].Item2));
                        }
                        else if (GameField[Position.Item1 + AttackDir[i].Item1, Position.Item2].ToLower() == "p" && AvailableEnPassent(EnemyPawn))
                        {
                            AvailableKillsList.Add((Position.Item1 + AttackDir[i].Item1, Position.Item2 + AttackDir[i].Item2));
                        }
                    }

                }
            }


            return AvailableKillsList;

        }
        private string pieces;

        public bool AvailableEnPassent(Pawn EnemyPawn)
        {
            bool IsEnemyPositionCorrect = EnemyPawn.Position == (EnemyPawn.StartPos.Item1 + EnemyMoveDir[1].Item1, EnemyPawn.StartPos.Item2 + EnemyMoveDir[1].Item2);
            bool IsVerticalPositionCorrect = Position.Item2 == EnemyPawn.Position.Item2;
            bool IsHorizontalposition1Correct = Position.Item1 == EnemyPawn.Position.Item1 + 1;
            bool IsHorizontalposition2Correct = Position.Item1 == EnemyPawn.Position.Item1 - 1;
            return EnemyPawn.EnPassantAvailable && IsEnemyPositionCorrect && (IsHorizontalposition1Correct || IsHorizontalposition2Correct) && IsVerticalPositionCorrect;
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

        public Pawn(PieceColor color, (int, int) position)
        {
            Color = color;
            if (Color == PieceColor.White)
            {
                MoveDir = new List<(int, int)> { (0, 1), (0, 2) };
                EnemyMoveDir = new List<(int, int)> { (0, -1), (0, -2) };
                AttackDir = new List<(int, int)> { (-1, 1), (1, 1) };
                Conditions = new Func<int, int, bool>[] {
                    (x,y)=> x>0 && y<7,
                    (x,y)=> x<7 && y<7
                };
                EndVerticalPos = ((int, int) CurrentPosition) => CurrentPosition.Item2 < 7;


            }
            else
            {

                MoveDir = new List<(int, int)> { (0, -1), (0, -2) };
                EnemyMoveDir = new List<(int, int)> { (0, 1), (0, 2) };
                AttackDir = new List<(int, int)> { (-1, -1), (1, -1) };

                Conditions = new Func<int, int, bool>[] {
                     (x, y) => x > 0 && y > 0,
                     (x, y) => x < 7 && y > 0
                };
                EndVerticalPos = ((int, int) CurrentPosition) => CurrentPosition.Item2 > 0;
            }
            StartPos = position;
            Position = StartPos;
            IsDead = false;
            EnPassantAvailable = false;
        }
    }
}
