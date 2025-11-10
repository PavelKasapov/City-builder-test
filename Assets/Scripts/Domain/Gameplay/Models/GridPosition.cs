using System;

namespace Domain.Gameplay.Models
{
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int X { get; }
        public int Y { get; }

        public GridPosition(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public bool Equals(GridPosition other) => this.X == other.X && this.Y == other.Y;
        public override bool Equals(object obj) => obj is GridPosition other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.X, this.Y);

        public static bool operator ==(GridPosition left, GridPosition right) => left.Equals(right);
        public static bool operator !=(GridPosition left, GridPosition right) => !left.Equals(right);

        public override string ToString() => $"({this.X}, {this.Y})";
    }
}
