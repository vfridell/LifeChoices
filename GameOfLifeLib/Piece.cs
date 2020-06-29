using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLifeLib.Rules;

namespace GameOfLifeLib
{

    /// <summary>
    /// A Piece must support all integer 
    /// </summary>
    public struct Piece
    {
        public int StateValue { get; private set; }
        public Owner Owner { get; private set; }
        public PieceAspect Aspect { get; private set; }

        public static Piece Get(int stateValue)=> Get(stateValue, Owner.None, PieceAspect.Natural);
        public static Piece Get(int stateValue, Owner owner) => Get(stateValue, owner, PieceAspect.Natural);
        public static Piece Get(int stateValue, Owner owner, PieceAspect aspect)
        {
            return new Piece() { StateValue = stateValue, Owner = owner, Aspect = aspect};
        }

        public static Piece Get(string s)
        {
            switch (s)
            {
                case "Dead":
                    return Get(0);
                case "AliveNoPlayer":
                    return Get(1, Owner.None);
                case "AlivePlayer1":
                    return Get(1, Owner.Player1);
                case "AlivePlayer2":
                    return Get(1, Owner.Player2);
                default:
                    throw new ArgumentException($"Invalid piece name: {s}");
            }
        }

    }
}
