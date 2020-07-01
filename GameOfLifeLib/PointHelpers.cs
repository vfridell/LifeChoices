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
    }
}
