using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib
{
    public enum PieceName { Dead = 0, Alive = 1 };
    public enum Owner { None = 0, Player1 = 1, Player2 = 2 };
    public enum PieceAspect { Natural = 0, Played = 1 }
    public struct Piece
    {
        public PieceName Name;
        public int Value;
        public Owner Owner;
        public PieceAspect Aspect;

        public static Piece Get(PieceName pieceName)=> Get(pieceName, Owner.None, PieceAspect.Natural);
        public static Piece Get(PieceName pieceName, Owner owner) => Get(pieceName, owner, PieceAspect.Natural);
        public static Piece Get(PieceName pieceName, Owner owner, PieceAspect aspect)
        {
            if (pieceName == PieceName.Dead) return new Piece();
            else return new Piece() { Name = PieceName.Alive, Owner = owner, Aspect = aspect, Value = 1};
        }

        public static Piece Get(string s)
        {
            switch (s)
            {
                case "Dead":
                    return Get(PieceName.Dead);
                case "AliveNoPlayer":
                    return Get(PieceName.Alive, Owner.None);
                case "AlivePlayer1":
                    return Get(PieceName.Alive, Owner.Player1);
                case "AlivePlayer2":
                    return Get(PieceName.Alive, Owner.Player2);
                default:
                    throw new ArgumentException($"Invalid piece name: {s}");
            }
        }

    }
}
