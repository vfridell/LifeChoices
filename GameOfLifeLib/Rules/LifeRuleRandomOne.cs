using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public class LifeRuleRandomOne : ICARule
    {
        int _xtraCount = 0;

        public LifeRuleRandomOne(int randomLiveToAdd)
        {
            _xtraCount = randomLiveToAdd;
        }
        public LifeRuleRandomOne()
        {
        }

        public PieceGrid Run(PieceGrid currentGen)
        {
            PieceGrid nextGen = currentGen.Clone();
            List<Point> tweakPoints = new List<Point>();
            foreach(var kvp in currentGen.PointPieces)
            {
                int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen).Sum(p => currentGen.PointPieces[p].StateValue);
                switch(kvp.Value.StateValue)
                {
                    case 1:
                        if (aliveNeighbors < 2 || aliveNeighbors > 3) nextGen.PointPieces[kvp.Key] = Piece.Get(0);
                        else nextGen.PointPieces[kvp.Key] = Piece.Get(1);
                        break;
                    case 0:
                        if (aliveNeighbors == 3) nextGen.PointPieces[kvp.Key] = Piece.Get(1);
                        else if (aliveNeighbors > 0)
                        {
                            tweakPoints.Add(kvp.Key);
                            nextGen.PointPieces[kvp.Key] = Piece.Get(0);
                        }
                        else nextGen.PointPieces[kvp.Key] = Piece.Get(0);
                        break;
                }
            }

            // randomly add a live point on the board
            Random rnd = new Random();
            int index = rnd.Next(0, tweakPoints.Count);
            if (_xtraCount > 0)
            {
                _xtraCount--;
                nextGen.PointPieces[tweakPoints[index]] = Piece.Get(1);
            }

            return nextGen;
        }
    }
}
