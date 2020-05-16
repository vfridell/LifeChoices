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
                int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen).Sum(p => currentGen.PointPieces[p].Value);
                switch(kvp.Value.Name)
                {
                    case PieceName.Alive:
                        if (aliveNeighbors < 2 || aliveNeighbors > 3) nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Dead);
                        else nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive);
                        break;
                    case PieceName.Dead:
                        if (aliveNeighbors == 3) nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive);
                        else if (aliveNeighbors > 0)
                        {
                            tweakPoints.Add(kvp.Key);
                            nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Dead);
                        }
                        else nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Dead);
                        break;
                }
            }

            // randomly add a live point on the board
            Random rnd = new Random();
            int index = rnd.Next(0, tweakPoints.Count);
            if (_xtraCount > 0)
            {
                _xtraCount--;
                nextGen.PointPieces[tweakPoints[index]] = Piece.Get(PieceName.Alive);
            }

            return nextGen;
        }
    }
}
