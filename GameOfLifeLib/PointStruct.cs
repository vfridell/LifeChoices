using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameOfLifeLib
{
    /// <summary>
    /// (X, Y) coordinates. Use the static <see cref="Point.Get(int, int)"/> function to get one
    /// </summary>
    public struct Point : IComparable<Point>, IComparable
    {
        public Point(int x, int y) { X = x; Y = y; }
        public int X;
        public int Y;

        public static Point operator +(Point p1, Point p2) => new Point(p1.X + p2.X, p1.Y + p2.Y);
        public static bool operator <(Point p1, Point p2) => p1.CompareTo(p2) < 0;
        public static bool operator >(Point p1, Point p2) => p1.CompareTo(p2) > 0;

        public void Add(Point p) { X += p.X; Y += p.Y; }
        public override bool Equals(object obj)
        {
            if (obj is Point) return Equals((Point)obj);
            else return false;
        }
        public bool Equals(Point other) => other.X == X && other.Y == Y;
        public static int Distance(Point p1, Point p2) => Math.Abs(p1.Y - p2.Y) + Math.Abs(p1.X - p2.X);
        public int Distance(Point p) => Math.Abs(p.Y - Y) + Math.Abs(p.X - X);
        public override int GetHashCode()
        {
            unchecked // prevent exceptions for int overflow
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public override string ToString() => $"({X},{Y})";

        public int CompareTo(Point other)
        {
            if (Equals(other)) return 0;
            if (Y < other.Y) return -1;
            else if (Y > other.Y) return 1;
            else if (X < other.X) return -1;
            else return 1;
        }

        public int CompareTo(object obj)
        {
            if (obj is Point) return CompareTo((Point)obj);
            else throw new ArgumentNullException("obj");
        }

        /// <summary>
        /// Get a Point 
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <returns>The Point object for the given coordinates</returns>
        public static Point Get(int x, int y) => new Point(x, y);

        private static Regex _stringPointRegex = new Regex(@"\(([0-9]+),([0-9]+)\)");
        public static Point Get(string s)
        {
            MatchCollection mc = _stringPointRegex.Matches(s);
            if (mc.Count == 0) throw new ArgumentException($"String not formatted as a point: {s}");
            return Point.Get(int.Parse(mc[0].Groups[1].Value), int.Parse(mc[0].Groups[2].Value));
        }
    }
}
