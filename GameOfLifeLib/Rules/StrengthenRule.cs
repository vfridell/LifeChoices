using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public class StrengthenRule : ICARule
    {
        public ICARule Rule1 { get; protected set; }
        public ICARule Rule2 { get; protected set; }

        public StrengthenRule(string name, ICARule rule1, ICARule rule2)
        {
            Name = name;
            Rule1 = rule1;
            Rule2 = rule2;
        }

        public int NumStates
        {
            get
            {
                return Math.Max(Rule1.NumStates, Rule2.NumStates);
            }
        }

        public string Name { get; protected set; }

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
            Piece p1 = Rule1.Run(currentGen, point);
            Piece p2 = Rule2.Run(currentGen, point);
            return p1.StateValue >= p2.StateValue ? p1 : p2;
        }
    }
}
