using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public class LifeRuleCenterTwo : ICARule
    {
        int _xtraCount1 = 0;
        int _xtraCount2 = 0;
        public HashSet<Point> tweakPoints1 = new HashSet<Point>();
        public HashSet<Point> tweakPoints2 = new HashSet<Point>();

        public string Name => "LifeRuleCenterTwo";

        public LifeRuleCenterTwo(int randomLiveToAdd)
        {
            _xtraCount1 = randomLiveToAdd;
            _xtraCount2 = randomLiveToAdd;
        }
        public LifeRuleCenterTwo()
        {
        }

        public PieceGrid Run(PieceGrid currentGen)
        {
            tweakPoints1.Clear();
            tweakPoints2.Clear();
            PieceGrid nextGen = currentGen.Clone();
            foreach(var kvp in currentGen.PointPieces)
            {
                nextGen.PointPieces[kvp.Key] = Run(currentGen, kvp.Key, kvp.Value);
            }

            // add live player cells on the board trying to increase at the center
            Point goalPoint = new Point(50, 50);
            if (tweakPoints1.Any() && _xtraCount1 > 0)
            {
                foreach(Point p in tweakPoints1.OrderBy(p => p.Distance(goalPoint)))
                {
                    int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(p, nextGen, PointHelpers.NeighborhoodOrder.Moore).Sum(n => nextGen.PointPieces[n].StateValue);
                    if (aliveNeighbors == 2)
                    {
                        nextGen.PointPieces[p] = Piece.Get(1, Owner.Player1, PieceAspect.Played);
                        _xtraCount1--;
                        break;
                    }
                }
            }

            if (tweakPoints2.Any() && _xtraCount2 > 0)
            {
                foreach (Point p in tweakPoints2.OrderBy(p => p.Distance(goalPoint)))
                {
                    int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(p, nextGen, PointHelpers.NeighborhoodOrder.Moore).Sum(n => nextGen.PointPieces[n].StateValue);
                    if (aliveNeighbors == 2)
                    {
                        nextGen.PointPieces[p] = Piece.Get(1, Owner.Player2, PieceAspect.Played);
                        _xtraCount2--;
                        break;
                    }
                }
            }
            return nextGen;
        }

        public Piece Run(PieceGrid currentGen, Point point) => Run(currentGen, point, currentGen.PointPieces[point]);

        private Piece Run(PieceGrid currentGen, Point point, Piece piece)
        {
            Piece returnPiece;

            int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(point, currentGen, PointHelpers.NeighborhoodOrder.Moore)
                .Sum(p => currentGen.PointPieces[p].StateValue);
            int aliveNeighbors1 = PointHelpers.GetAdjacentPointsToroid(point, currentGen, PointHelpers.NeighborhoodOrder.Moore)
                .Where(p => currentGen.PointPieces[p].Owner == Owner.Player1)
                .Sum(p => currentGen.PointPieces[p].StateValue);
            int aliveNeighbors2 = PointHelpers.GetAdjacentPointsToroid(point, currentGen, PointHelpers.NeighborhoodOrder.Moore)
                .Where(p => currentGen.PointPieces[p].Owner == Owner.Player2)
                .Sum(p => currentGen.PointPieces[p].StateValue);
            switch (piece.StateValue)
            {
                case 1:
                    if (aliveNeighbors < 2 || aliveNeighbors > 3) returnPiece = Piece.Get(0);
                    else returnPiece = Piece.Get(1);
                    break;
                case 0:
                    bool added = false;
                    if (aliveNeighbors == 3)
                    {
                        added = true;
                        if (aliveNeighbors1 > aliveNeighbors2)
                            returnPiece = Piece.Get(1, Owner.Player1);
                        else if (aliveNeighbors2 > aliveNeighbors1)
                            returnPiece = Piece.Get(1, Owner.Player2);
                        else
                            returnPiece = Piece.Get(1, Owner.None);
                    }
                    else
                    {
                        returnPiece = Piece.Get(0);
                    }

                    if (aliveNeighbors1 > 0 && !added)
                    {
                        tweakPoints1.Add(point);
                    }
                    if (aliveNeighbors2 > 0 && !added)
                    {
                        tweakPoints2.Add(point);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            return returnPiece;
        }
    }
}
