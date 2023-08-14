using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib
{
    public static class PointHelpers
    {
        public enum NeighborhoodOrder { Moore, MooreRuleTree, MooreRuleTable, VonNeumann, VonNeumannRuleTree, VonNeumannRuleTable };
        public enum GridDirection { NorthWest=0, NorthEast=1, SouthWest=2, SouthEast=3, North=4, West=5, East=6, South=7 };
        public static Point[] AdjacentPointDeltas = {
            Point.Get(-1,1),
            Point.Get(1,1),
            Point.Get(-1,-1),
            Point.Get(1,-1),
            Point.Get(0,1),
            Point.Get(-1,0),
            Point.Get(1,0),
            Point.Get(0,-1),
        };
        
        // neighborhood orders
        private static int[] RuleTreeMoore = new int[8] { 0,1,2,3,4,5,6,7 };
        private static int[] RuleTreeVonNeumann = new int[4] { 4,5,6,7 };
        private static int[] RuleTableMoore  = new int[8] { 4,1,6,3,7,2,5,0 };
        private static int[] RuleTableVonNeumann = new int[4] { 4,6,7,5 };

        public static int[] GetRuleOrderArray(NeighborhoodOrder orderEnum)
        {
            switch(orderEnum)
            {
                case NeighborhoodOrder.Moore:
                case NeighborhoodOrder.MooreRuleTree:
                    return RuleTreeMoore;

                case NeighborhoodOrder.MooreRuleTable:
                    return RuleTableMoore;

                case NeighborhoodOrder.VonNeumann:
                case NeighborhoodOrder.VonNeumannRuleTree:
                    return RuleTreeVonNeumann;

                case NeighborhoodOrder.VonNeumannRuleTable:
                    return RuleTableVonNeumann;

                default:
                    throw new NotImplementedException();
            }
        }

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
        public static IEnumerable<Point> GetAdjacentPointsToroid(this Point centerPoint, PieceGrid grid, NeighborhoodOrder neighborhoodOrder)
        {
            int[] order = GetRuleOrderArray(neighborhoodOrder);

            Point next;
            for (int i = 0; i < order.Length; i++)
            {
                next = centerPoint;
                next.Add(AdjacentPointDeltas[order[i]]);
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

        public static IEnumerable<Point> GetAdjacentPointsNotOutOfBounds(this Point centerPoint, PieceGrid grid, NeighborhoodOrder neighborhoodOrder)
        {
            int[] order = GetRuleOrderArray(neighborhoodOrder);

            Point next;
            for (int i = 0; i < order.Length; i++)
            {
                next = centerPoint;
                next.Add(AdjacentPointDeltas[order[i]]);
                if (grid.IsOutOfBounds(next)) continue;
                yield return next;
            }
        }

        public static Point AddPointsToroid(this Point point1, Point point2, PieceGrid grid)
        {
            Point next;
            next = point1 + point2;
            if (grid.IsOutOfBounds(next))
            {
                if (next.X < 0) next.X = (grid.Size) + next.X;
                if (next.Y < 0) next.Y = (grid.Size) + next.Y;
                if (next.X > grid.Size - 1) next.X = next.X - (grid.Size);
                if (next.Y > grid.Size - 1) next.Y = next.Y - (grid.Size);
            }
            return next;
        }



        public static List<HashSet<int>> GetIsoPoints(List<GridDirection> dirs)
        {
            var result = new List<HashSet<int>>();

            Func<Func<int, int>, HashSet<int>> IsoCheck = (t) =>
            {
                var hs = new HashSet<int>();
                foreach (var dir in dirs)
                {
                    int pt = t((int)dir);
                    if (hs.Contains(pt)) throw new Exception("AAA");
                    hs.Add(pt);
                }
                return hs;
            };
            result.Add(IsoCheck(Equal));
            var p = IsoCheck(Rotate90);
            if (!result.Any(h => h.SetEquals(p))) result.Add(p);
            p = IsoCheck(MirrorDiag1);
            if (!result.Any(h => h.SetEquals(p))) result.Add(p);
            p = IsoCheck(MirrorDiag2);
            if (!result.Any(h => h.SetEquals(p))) result.Add(p);
            p = IsoCheck(MirrorVert);
            if (!result.Any(h => h.SetEquals(p))) result.Add(p);
            p = IsoCheck(MirrorHoriz);
            if (!result.Any(h => h.SetEquals(p))) result.Add(p);
            p = IsoCheck(Rotate180);
            if (!result.Any(h => h.SetEquals(p))) result.Add(p);
            p = IsoCheck(Rotate270);
            if (!result.Any(h => h.SetEquals(p))) result.Add(p);
            return result;
        }

        // these only work for NeighborhoodOrder.Moore: and NeighborhoodOrder.MooreRuleTree
        // 0|4|1
        // _____
        // 5|x|6
        // -----
        // 2|7|3
        public static int Equal(int pos) => pos;
        public static int MirrorDiag2(int pos)
        {
            switch (pos)
            {
                case 0: return 0;
                case 1: return 2;
                case 2: return 1;
                case 3: return 3;
                case 4: return 5;
                case 5: return 4;
                case 6: return 7;
                case 7: return 6;
                default: throw new NotImplementedException();
            };
        }
        public static int MirrorDiag1(int pos)
        {
            switch (pos)
            {
                case 0: return 3;
                case 1: return 1;
                case 2: return 2;
                case 3: return 0;
                case 4: return 6;
                case 5: return 7;
                case 6: return 4;
                case 7: return 5;
                default: throw new NotImplementedException();
            };
        }

        public static int MirrorVert(int pos)
        {
            switch (pos)
            {
                case 4: return 7;
                case 0: return 2;
                case 1: return 3;
                case 2: return 0;
                case 5: return 5;
                case 3: return 1;
                case 7: return 4;
                case 6: return 6;
                default: throw new NotImplementedException();
            };
        }

        public static int MirrorHoriz(int pos)
        {
            switch (pos)
            {
                case 4: return 4;
                case 0: return 1;
                case 1: return 0;
                case 2: return 3;
                case 3: return 2;
                case 6: return 5;
                case 7: return 7;
                case 5: return 6;
                default: throw new NotImplementedException();
            };
        }

        public static int Rotate90(int pos)
        {
            switch (pos)
            {
                case 4: return 6;
                case 0: return 1;
                case 1: return 3;
                case 2: return 0;
                case 5: return 4;
                case 7: return 5;
                case 6: return 7;
                case 3: return 2;
                default: throw new NotImplementedException();
            };
        }

        public static int Rotate180(int pos) => Rotate90(Rotate90(pos));
        public static int Rotate270(int pos) => Rotate90(Rotate90(Rotate90(pos)));
    }
}
