using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public class HighLifeRule : ICARule
    {
        public string Name => "HighLifeRule";
        public PieceGrid Run(PieceGrid currentGen)
        {
            PieceGrid nextGen = currentGen.Clone();
            foreach(var kvp in currentGen.PointPieces)
            {

            }
            return nextGen;
        }


        public Piece Run(PieceGrid currentGen, Point point) => Run(currentGen, point, currentGen.PointPieces[point]);


        public Piece Run(PieceGrid currentGen, Point point, Piece piece)
        {
            int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(point, currentGen, PointHelpers.NeighborhoodOrder.Moore).Sum(p => currentGen.PointPieces[p].StateValue);
            switch (piece.StateValue)
            {
                case 1:
                    if (aliveNeighbors < 2 || aliveNeighbors > 3) return Piece.Get(0);
                    else return Piece.Get(1);
                case 0:
                    if (aliveNeighbors == 3 || aliveNeighbors == 6) return Piece.Get(1);
                    else return Piece.Get(0);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
