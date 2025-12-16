using System;

namespace ChessLib
{
    /// <summary>
    /// Represents a position on the chess board
    /// </summary>
    public struct Position : IEquatable<Position>
    {
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Validates that the position is within board bounds (0-7)
        /// </summary>
        public bool IsValid()
        {
            return X >= 0 && X < 8 && Y >= 0 && Y < 8;
        }

        /// <summary>
        /// Throws exception if position is out of bounds
        /// </summary>
        public void Validate()
        {
            if (!IsValid())
                throw new ArgumentOutOfRangeException($"Position coordinates must be between 0 and 7. Got: ({X}, {Y})");
        }

        public static implicit operator Position((int x, int y) tuple)
        {
            return new Position(tuple.x, tuple.y);
        }

        public static implicit operator (int, int)(Position position)
        {
            return (position.X, position.Y);
        }

        public bool Equals(Position other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Position other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}

