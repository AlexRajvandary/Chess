using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChessLib
{
    public abstract class PieceBase : IPiece
    {
#if DEBUG
        private static readonly ConcurrentDictionary<(int X, int Y), long> _callsByPosition = new();
        private static readonly ConcurrentDictionary<(int X, int Y), byte> _loggedByPosition = new();

        protected void TrackAvailableMoves(long threshold = 300, bool traceStack = false)
        {
            var pos = (Position.X, Position.Y);
            var total = _callsByPosition.AddOrUpdate(pos, 1, static (_, v) => v + 1);
            
            //there is a cascade calls 396 times per one piece, needs to be refactored
            if (total >= threshold && _loggedByPosition.TryAdd(pos, 0))
            {
                CustomTraceLogger.TraceCaller($"{Color} {GetType().Name} ({pos.X},{pos.Y}) reached {total} calls");
                if (traceStack)
                {
                    CustomTraceLogger.TraceStack($"{Color} {GetType().Name} ({pos.X},{pos.Y}) reached {total} calls", take: 25);
                }
            }
        }

        public static void ResetAvailableMovesDiagnostics()
        {
            _callsByPosition.Clear();
            _loggedByPosition.Clear();
        }
#endif

        public abstract PieceColor Color { get; set; }
        public abstract Position Position { get; set; }
        public abstract bool IsDead { get; set; }

        public abstract List<Position> AvailableKills(string[,] GameField);
        public abstract List<Position> AvailableMoves(string[,] GameField);
        public abstract void ChangePosition(Position position);
        public abstract object Clone();
    }
}