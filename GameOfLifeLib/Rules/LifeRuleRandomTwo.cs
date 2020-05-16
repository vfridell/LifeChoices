using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public class LifeRuleRandomTwo : ICARule
    {
        int _xtraCount1 = 0;
        int _xtraCount2 = 0;

        public LifeRuleRandomTwo(int randomLiveToAdd)
        {
            _xtraCount1 = randomLiveToAdd;
            _xtraCount2 = randomLiveToAdd;
        }
        public LifeRuleRandomTwo()
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
                        if (aliveNeighbors == 3)
                        {
                            if(aliveNeighbors1 > aliveNeighbors2)
                                nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive, Owner.Player1);
                            else if(aliveNeighbors2 > aliveNeighbors1)
                                nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive, Owner.Player2);
                            else
                                nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive, Owner.None);
                        }
                        else if (aliveNeighbors == 6 && aliveNeighbors1 > 0 && aliveNeighbors2 > 0)
                        {
                            nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive, Owner.None);
                        }
                        else
                        {
                            nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Dead);
                        }

                        if (aliveNeighbors1 > 0)
                        {
                            tweakPoints1.Add(kvp.Key);
                        }
                        if (aliveNeighbors2 > 0)
                        {
                            tweakPoints2.Add(kvp.Key);
                        }
                        break;
                }
            }

            // randomly add live player cells on the board
            Random rnd = new Random();
            if (tweakPoints1.Any() && _xtraCount1 > 0)
            {
                _xtraCount1--;
                int index1 = rnd.Next(0, tweakPoints1.Count - 1);
                nextGen.PointPieces[tweakPoints1[index1]] = Piece.Get(PieceName.Alive, Owner.Player1);
            }
            if (tweakPoints2.Any() && _xtraCount2 > 0)
            {
                _xtraCount2--;
                int index2 = rnd.Next(0, tweakPoints2.Count - 1);
                nextGen.PointPieces[tweakPoints2[index2]] = Piece.Get(PieceName.Alive, Owner.Player2);
            }
            return nextGen;
        }
    }
}
