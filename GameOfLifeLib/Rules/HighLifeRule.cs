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
                int aliveNeighbors = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen).Sum(p => currentGen.PointPieces[p].Value);
                switch(kvp.Value.Name)
                {
                    case PieceName.Alive:
                        if (aliveNeighbors < 2 || aliveNeighbors > 3) nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Dead);
                        else nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive);
                        break;
                    case PieceName.Dead:
                        if (aliveNeighbors == 3 || aliveNeighbors == 6) nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Alive);
                        else nextGen.PointPieces[kvp.Key] = Piece.Get(PieceName.Dead);
                        break;
                }
            }
            return nextGen;
        }
    }
}
