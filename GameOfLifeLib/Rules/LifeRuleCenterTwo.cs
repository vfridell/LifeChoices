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
            PieceGrid nextGen = currentGen.Clone();
            List<Point> tweakPoints1 = new List<Point>();
            List<Point> tweakPoints2 = new List<Point>();
            foreach(var kvp in currentGen.PointPieces)
            {
                int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen)
                    .Sum(p => currentGen.PointPieces[p].Value);
                int aliveNeighbors1 = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen)
                    .Where(p => currentGen.PointPieces[p].Owner == Owner.Player1)
                    .Sum(p => currentGen.PointPieces[p].Value);
                int aliveNeighbors2 = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen)
                    .Where(p => currentGen.PointPieces[p].Owner == Owner.Player2)
                    .Sum(p => currentGen.PointPieces[p].Value);
                switch(kvp.Value.Name)
                {
                    case PieceName.Alive:
                        if (aliveNeighbors < 2 || aliveNeighbors > 3) nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Dead);
                        else nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive, kvp.Value.Owner);
                        break;
                    case PieceName.Dead:
                        bool added = false;
                        if (aliveNeighbors == 3)
                        {
                            added = true;
                            if(aliveNeighbors1 > aliveNeighbors2)
                                nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive, Owner.Player1);
                            else if(aliveNeighbors2 > aliveNeighbors1)
                                nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive, Owner.Player2);
                            else
                                nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive, Owner.None);
                        }
                        //else if (aliveNeighbors == 6 && aliveNeighbors1 > 0 && aliveNeighbors2 > 0)
                        //{
                        //    added = true;
                        //    nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive, Owner.None);
                        //}
                        else
                        {
                            nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Dead);
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
                    int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(p, nextGen).Sum(n => nextGen.PointPieces[n].Value);
                    if (aliveNeighbors == 2)
                    {
                        nextGen.PointPieces[p] = Piece.Get(PieceName.Alive, Owner.None);
                        _xtraCount1--;
                        break;
                    }
                }
            }

            if (tweakPoints2.Any() && _xtraCount2 > 0)
            {
                foreach (Point p in tweakPoints2.OrderBy(p => p.Distance(goalPoint)))
                {
                    int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(p, nextGen).Sum(n => nextGen.PointPieces[n].Value);
                    if (aliveNeighbors == 2)
                    {
                        nextGen.PointPieces[p] = Piece.Get(PieceName.Alive, Owner.None);
                        _xtraCount2--;
                        break;
                    }
                }
            }
            return nextGen;
        }
    }
}
