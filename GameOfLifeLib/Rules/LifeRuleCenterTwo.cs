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
                int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen)
                    .Sum(p => currentGen.PointPieces[p].StateValue);
                int aliveNeighbors1 = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen)
                    .Where(p => currentGen.PointPieces[p].Owner == Owner.Player1)
                    .Sum(p => currentGen.PointPieces[p].StateValue);
                int aliveNeighbors2 = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen)
                    .Where(p => currentGen.PointPieces[p].Owner == Owner.Player2)
                    .Sum(p => currentGen.PointPieces[p].StateValue);
                switch(kvp.Value.StateValue)
                {
                    case 1:
                        if (aliveNeighbors < 2 || aliveNeighbors > 3) nextGen.PointPieces[kvp.Key] = Piece.Get(0);
                        else nextGen.PointPieces[kvp.Key] = Piece.Get(1, kvp.Value.Owner);
                        break;
                    case 0:
                        bool added = false;
                        if (aliveNeighbors == 3)
                        {
                            added = true;
                            if(aliveNeighbors1 > aliveNeighbors2)
                                nextGen.PointPieces[kvp.Key] = Piece.Get(1, Owner.Player1);
                            else if(aliveNeighbors2 > aliveNeighbors1)
                                nextGen.PointPieces[kvp.Key] = Piece.Get(1, Owner.Player2);
                            else
                                nextGen.PointPieces[kvp.Key] = Piece.Get(1, Owner.None);
                        }
                        //else if (aliveNeighbors == 6 && aliveNeighbors1 > 0 && aliveNeighbors2 > 0)
                        //{
                        //    added = true;
                        //    nextGen.PointPieces[kvp.Key] = Piece.Get(1, Owner.None);
                        //}
                        else
                        {
                            nextGen.PointPieces[kvp.Key] = Piece.Get(0);
                        }

                        if (aliveNeighbors1 > 0 && !added)
                        {
                            tweakPoints1.Add(kvp.Key);
                        }
                        if (aliveNeighbors2 > 0 && !added)
                        {
                            tweakPoints2.Add(kvp.Key);
                        }
                        break;
                }
            }

            // add live player cells on the board trying to increase at the center
            Point goalPoint = new Point(50, 50);
            if (tweakPoints1.Any() && _xtraCount1 > 0)
            {
                foreach(Point p in tweakPoints1.OrderBy(p => p.Distance(goalPoint)))
                {
                    int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(p, nextGen).Sum(n => nextGen.PointPieces[n].StateValue);
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
                    int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(p, nextGen).Sum(n => nextGen.PointPieces[n].StateValue);
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
    }
}
