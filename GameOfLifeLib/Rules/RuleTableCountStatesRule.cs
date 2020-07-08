using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLifeLib.Parsers;

namespace GameOfLifeLib.Rules
{
    public class RuleTableCountStatesRule : ICARule
    {
        public string Name { get; protected set; }
        public RuleSymmetry Symmetry { get; private set; }
        public CANeighborhood Neighborhood { get; private set; }
        public int NumStates { get; private set; }
        public Dictionary<string, int> RuleDictionary { get; set; }

        public RuleTableCountStatesRule(string name, CANeighborhood neighborhood, RuleSymmetry symmetry, int states, Dictionary<string, int> ruleDictionary)
        {
            RuleDictionary = ruleDictionary;
            Neighborhood = neighborhood;
            Symmetry = symmetry;
            NumStates = states;
        }

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
            IEnumerable<Point> neighborhoodPoints = PointHelpers.GetAdjacentPointsToroid(point, currentGen, PointHelpers.NeighborhoodOrder.MooreRuleTable);
            List<int> neighborhood = new List<int>();
            foreach (Point p in neighborhoodPoints) neighborhood.Add(currentGen.PointPieces[p].StateValue);

            List<int> sortedNeighborhood = neighborhood.OrderBy(s => s).ToList();
            sortedNeighborhood.Insert(0, piece.StateValue);

            if (RuleDictionary.TryGetValue(GetKeyString(sortedNeighborhood), out int stateValue))
            {
                return Piece.Get(stateValue);
            }
            else
            {
                return piece;
            }
        }

        private string GetKeyString<T>(IEnumerable<T> neighborhood) => neighborhood.Skip(1).Aggregate(neighborhood.First().ToString(), (s2, s) => s2 + "," + s.ToString());
    }
}
