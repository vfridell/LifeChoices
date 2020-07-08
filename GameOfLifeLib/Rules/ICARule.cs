using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public interface ICARule
    {
        int NumStates { get; }
        string Name { get; }
        PieceGrid Run(PieceGrid currentGen);
        Piece Run(PieceGrid grid, Point point);
    }
}
