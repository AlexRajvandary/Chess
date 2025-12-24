using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ChessLib
{
    public static class CustomTraceLogger
    {
        private static int _counter;

        public static void TraceCaller(
            string message,
            int skipFrames = 1,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
#if DEBUG
            _counter++;

            // skipFrames=1 пропускает сам TraceCaller
            var st = new StackTrace(skipFrames, fNeedFileInfo: true);
            var frames = st.GetFrames();

            string caller = "<unknown>";
            if (frames != null)
            {
                // Ищем первый кадр НЕ из Pawn и НЕ из системных/ WPF сборок
                foreach (var f in frames)
                {
                    var m = f.GetMethod();
                    var dt = m?.DeclaringType;
                    var typeName = dt?.FullName ?? "";
                    var methodName = m?.Name ?? "";

                    if (string.IsNullOrWhiteSpace(typeName))
                        continue;

                    // отсекаем инфраструктуру
                    if (typeName.StartsWith("System.", StringComparison.Ordinal) ||
                        typeName.StartsWith("Microsoft.", StringComparison.Ordinal) ||
                        typeName.StartsWith("MS.", StringComparison.Ordinal) ||
                        typeName.StartsWith("PresentationFramework", StringComparison.Ordinal) ||
                        typeName.StartsWith("WindowsBase", StringComparison.Ordinal))
                        continue;

                    // отсекаем сам класс Pawn (чтобы найти реального вызвавшего)
                    if (typeName.Contains(".Pawn", StringComparison.Ordinal) || typeName.EndsWith("Pawn", StringComparison.Ordinal))
                        continue;

                    caller = $"{typeName}.{methodName}";
                    break;
                }
            }

            Trace.WriteLine($"{Path.GetFileName(file)}:{line} #{_counter} caller={caller} → {message}");
#endif
        }

        public static void TraceStack(
            string message,
            int take = 12,
            int skipFrames = 1,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
#if DEBUG
            _counter++;
            var st = new StackTrace(skipFrames, fNeedFileInfo: true);
            var frames = st.GetFrames() ?? Array.Empty<StackFrame>();

            var lines = frames
                .Select(f => f.GetMethod())
                .Where(m => m != null)
                .Select(m => $"{m!.DeclaringType?.FullName}.{m.Name}")
                .Take(take);

            Trace.WriteLine($"{Path.GetFileName(file)}:{line} #{_counter} stack:\n  {string.Join("\n  ", lines)}\n  → {message}");
#endif
        }
    }

    public class Pawn : IPiece
    {
        private static int _logged;
        public bool IsDead { get; set; }
        public PieceColor Color { get; set; }
        public Position Position { get; set; }

        /// <summary>
        /// Starting position, needed to check available moves for pawn (if pawn is in starting position, there are 2 move options)
        /// </summary>
        public Position StartPos;
        private Func<Position, bool> EndVerticalPos;
        private readonly Position[] DirectionsForMove = new Position[] { new Position(0, 1), new Position(0, -1) };

        public List<Position> MoveDir { get; set; }
        private List<Position> EnemyMoveDir { get; set; }
        /// <summary>
        /// Checks all available moves for current pawn
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns></returns>
        /// <returns></returns>
        public List<Position> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<Position>();
            //if (_logged++ < 10)
            //{
            //    CustomTraceLogger.TraceStack("AvailableMoves called", take: 20);
            //}
            if (Position == StartPos)
            {
                var move1 = new Position(Position.X + MoveDir[0].X, Position.Y + MoveDir[0].Y);
                if (GameField[move1.X, move1.Y] == " ")
                {
                    AvailableMovesList.Add(move1);
                    var move2 = new Position(Position.X + MoveDir[1].X, Position.Y + MoveDir[1].Y);
                    if (GameField[move2.X, move2.Y] == " ")
                    {
                        AvailableMovesList.Add(move2);
                    }
                }
            }
            else
            {
                if (EndVerticalPos(Position))
                {
                    var move = new Position(Position.X + MoveDir[0].X, Position.Y + MoveDir[0].Y);
                    if (GameField[move.X, move.Y] == " ")
                    {
                        AvailableMovesList.Add(move);
                    }
                }
            }
            return AvailableMovesList;
        }

        public bool EnPassantAvailable { get; set; }

        public override string ToString()
        {
            return Color == PieceColor.White ? "P" : "p"; ;
        }

        /// <summary>
        /// Attack directions
        /// </summary>
        private List<Position> AttackDir { get; set; }

        /// <summary>
        /// Attack conditions
        /// </summary>
        private Func<int, int, bool>[] Conditions { get; set; }

        /// <summary>
        /// Finds available enemy pieces for attack
        /// </summary>
        /// <param name="GameField">Game field</param>
        /// <returns>Returns list of coordinates of pieces available for attack</returns>
        public List<Position> AvailableKills(string[,] GameField)
        {
            var AvailableKillsList = new List<Position>();

            GetOppositeAndFriendPieces();
            for (int i = 0; i < 2; i++)
            {
                if (Conditions[i](Position.X, Position.Y))
                {
                    var attackPos = new Position(Position.X + AttackDir[i].X, Position.Y + AttackDir[i].Y);
                    if (GameField[attackPos.X, attackPos.Y] != " " && pieces.Contains(GameField[attackPos.X, attackPos.Y]))
                    {
                        AvailableKillsList.Add(attackPos);
                    }
                }
            }

            return AvailableKillsList;
        }

        public List<Position> AvailableKills(string[,] GameField, Pawn EnemyPawn)
        {
            var AvailableKillsList = new List<Position>();

            GetOppositeAndFriendPieces();
            if (EnemyPawn is null)
            {
                AvailableKillsList = AvailableKills(GameField);
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    if (Conditions[i](Position.X, Position.Y))
                    {
                        var attackPos = new Position(Position.X + AttackDir[i].X, Position.Y + AttackDir[i].Y);
                        if (GameField[attackPos.X, attackPos.Y] != " " && pieces.Contains(GameField[attackPos.X, attackPos.Y]))
                        {
                            AvailableKillsList.Add(attackPos);
                        }
                        else if (GameField[attackPos.X, Position.Y].ToLower() == "p" && AvailableEnPassent(EnemyPawn))
                        {
                            AvailableKillsList.Add(attackPos);
                        }
                    }
                }
            }

            return AvailableKillsList;
        }
        private string pieces;

        public bool AvailableEnPassent(Pawn EnemyPawn)
        {
            var enemyTargetPos = new Position(EnemyPawn.StartPos.X + EnemyMoveDir[1].X, EnemyPawn.StartPos.Y + EnemyMoveDir[1].Y);
            bool IsEnemyPositionCorrect = EnemyPawn.Position == enemyTargetPos;
            bool IsVerticalPositionCorrect = Position.Y == EnemyPawn.Position.Y;
            bool IsHorizontalposition1Correct = Position.X == EnemyPawn.Position.X + 1;
            bool IsHorizontalposition2Correct = Position.X == EnemyPawn.Position.X - 1;
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

        public void ChangePosition(Position position)
        {
            this.Position = position;
        }

        public object Clone()
        {
            return new Pawn(Color, Position);
        }

        public Pawn(PieceColor color, Position position)
        {
            Color = color;
            if (Color == PieceColor.White)
            {
                MoveDir = new List<Position> { new Position(0, 1), new Position(0, 2) };
                EnemyMoveDir = new List<Position> { new Position(0, -1), new Position(0, -2) };
                AttackDir = new List<Position> { new Position(-1, 1), new Position(1, 1) };
                Conditions = new Func<int, int, bool>[] {
                    (x,y)=> x>0 && y<7,
                    (x,y)=> x<7 && y<7
                };
                EndVerticalPos = (Position CurrentPosition) => CurrentPosition.Y < 7;
            }
            else
            {
                MoveDir = new List<Position> { new Position(0, -1), new Position(0, -2) };
                EnemyMoveDir = new List<Position> { new Position(0, 1), new Position(0, 2) };
                AttackDir = new List<Position> { new Position(-1, -1), new Position(1, -1) };

                Conditions = new Func<int, int, bool>[] {
                     (x, y) => x > 0 && y > 0,
                     (x, y) => x < 7 && y > 0
                };
                EndVerticalPos = (Position CurrentPosition) => CurrentPosition.Y > 0;
            }
            StartPos = position;
            Position = StartPos;
            IsDead = false;
        }
    }
}