using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib
{
    public static class PointHelpers
    {
        public enum GridDirection { UpLeft, Up, UpRight, Left, Right, DownLeft, Down, DownRight };
        public static Point[] AdjacentPointDeltas = {
            Point.Get(-1,1),
            Point.Get(0,1),
            Point.Get(1,1),
            Point.Get(-1,0),
            Point.Get(1,0),
            Point.Get(-1,-1),
            Point.Get(0,-1),
            Point.Get(1,-1),
        };
        public static IEnumerable<Point> GetAdjacentPoints(this Point centerPoint)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return AdjacentPointDeltas[i] + centerPoint;
            }
        }
        public static IEnumerable<Point> GetAdjacentDeltas(this Point centerPoint)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return AdjacentPointDeltas[i];
            }
        }
        public static IEnumerable<Point> GetAdjacentPointsToroid(this Point centerPoint, PieceGrid grid)
        {
            Point next;
            for (int i = 0; i < 8; i++)
            {
                next = centerPoint;
                next.Add(AdjacentPointDeltas[i]);
                if(grid.IsOutOfBounds(next))
                {
                    if (next.X < 0) next.X = grid.Size - 1;
                    if (next.Y < 0) next.Y = grid.Size - 1;
                    if (next.X > grid.Size - 1) next.X = 0;
                    if (next.Y > grid.Size - 1) next.Y = 0;
                }
                yield return next;
            }
        }
    }
}
