using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public class LifeLikeRule : ICARule
    {
        public List<int> BirthNeighborCounts { get; protected set; }
        public List<int> SurvivalNeighborCounts { get; protected set; }
        public int NumStates => 2;
        public string Name { get; protected set; }

        private static Regex _ruleStringRegex = new Regex("^B([0-8]+)/S([0-8]+)$|^B([0-8]+)$");
        
        public LifeLikeRule(string name, string ruleString)
        {
            Name = name;
            BirthNeighborCounts = new List<int>();
            SurvivalNeighborCounts = new List<int>();
            ParseRuleStringRegex(ruleString);
        }

        private void ParseRuleStringRegex(string ruleString)
        {
            var matchColl = _ruleStringRegex.Matches(ruleString);
            foreach(char c in matchColl[0].Groups[1].Value)
            {
                BirthNeighborCounts.Add(int.Parse(c.ToString()));
            }
            foreach(char c in matchColl[0].Groups[2].Value)
            {
                SurvivalNeighborCounts.Add(int.Parse(c.ToString()));
            }
            foreach (char c in matchColl[0].Groups[3].Value)
            {
                BirthNeighborCounts.Add(int.Parse(c.ToString()));
            }

        }

        public static bool LifeLikeRuleString(string ruleString) => _ruleStringRegex.IsMatch(ruleString);

        public PieceGrid Run(PieceGrid currentGen)
        {
            PieceGrid nextGen = currentGen.Clone();
            foreach (var kvp in currentGen.PointPieces)
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
                    if (BirthNeighborCounts.Contains(aliveNeighbors)) return Piece.Get(1);
                    else return Piece.Get(0);
                default:
                case 1:
                    if (SurvivalNeighborCounts.Contains(aliveNeighbors)) return Piece.Get(1);
                    else return Piece.Get(0);
                 //   throw new NotImplementedException();
            }
        }
    }
}
