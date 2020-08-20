using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public class LifeRule : ICARule
    {
        public int NumStates => 2;
        public string Name => "Life";

        public PieceGrid Run(PieceGrid currentGen)
        {
            PieceGrid nextGen = currentGen.Clone();
            foreach(var kvp in currentGen.PointPieces)
            {
                nextGen.PointPieces[kvp.Key] = Run(currentGen, kvp.Key, kvp.Value);
            }
            return nextGen;
        }

        public Piece Run(PieceGrid currentGen, Point point) => Run(currentGen, point, currentGen.PointPieces[point]);

        public Piece Run(PieceGrid currentGen, Point point, Piece piece)
        {
            int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(point, currentGen, PointHelpers.NeighborhoodOrder.Moore).Count(p => currentGen.PointPieces[p].StateValue > 0);
            switch (piece.StateValue)
            {
                case 0:
                    if (aliveNeighbors == 3) return Piece.Get(1);
                    else return Piece.Get(0);
                case 1:
                default:
                    if (aliveNeighbors < 2 || aliveNeighbors > 3) return Piece.Get(0);
                    else return Piece.Get(1);
            }
        }
    }
}
