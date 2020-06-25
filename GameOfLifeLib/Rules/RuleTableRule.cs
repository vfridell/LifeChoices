using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLifeLib.Parsers;

namespace GameOfLifeLib.Rules
{
    public class RuleTableRule : ICARule
    {
        public RuleSymmetry Symmetry { get; private set; }
        public CANeighborhood Neighborhood { get; private set; }
        public int States { get; private set; }
        public List<List<int>> RuleTable { get; set; }

        public RuleTableRule(CANeighborhood neighborhood, RuleSymmetry symmetry, int states, List<List<int>> ruleTable)
        {
            RuleTable = ruleTable;
            Neighborhood = neighborhood;
            Symmetry = symmetry;
            States = states;
        }

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
