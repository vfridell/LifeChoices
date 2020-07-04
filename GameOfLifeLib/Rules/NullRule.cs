using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public class NullRule : ICARule
    {
        public PieceGrid Run(PieceGrid currentGen)
        {
            PieceGrid nextGen = currentGen.Clone();
            return nextGen;
        }

        public Piece Run(PieceGrid grid, Point point)
        {
            return grid.PointPieces[point];
        }
    }
}
