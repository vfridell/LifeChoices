using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public class HighLifeRule : ICARule
    {
        public PieceGrid Run(PieceGrid currentGen)
        {
            PieceGrid nextGen = currentGen.Clone();
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
                        if (aliveNeighbors == 3 || aliveNeighbors == 6) nextGen.PointPieces[kvp.Key] = Piece.Get(1);
                        else nextGen.PointPieces[kvp.Key] = Piece.Get(0);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return nextGen;
        }
    }
}
