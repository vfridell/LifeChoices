using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public interface ICARule
    {
        PieceGrid Run(PieceGrid currentGen);
        Piece Run(PieceGrid grid, Point point);
    }
}
