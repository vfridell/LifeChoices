using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib
{
    public enum PieceName { Dead, Alive };
    public enum Owner { None = 0, Player1 = 1, Player2 = 2 };
    public abstract class Piece
    {
        protected Piece() { }

        public abstract PieceName Name { get; }
        public abstract int Value { get; }
        public abstract Owner Owner { get; }

        protected static Dead _dead = new Dead();

        protected static Piece[] _livePieces = new Piece[3];
        static Piece()
        {
            _livePieces[0] = new AliveNoPlayer();
            _livePieces[1] = new AlivePlayer1();
            _livePieces[2] = new AlivePlayer2();
        }

        public static Piece Get(PieceName pieceName)
        {
            if (pieceName == PieceName.Dead) return _dead;
            else return _livePieces[0];
        }
        public static Piece Get(PieceName pieceName, Owner owner)
        {
            if (pieceName == PieceName.Dead) return _dead;
            return _livePieces[(int)owner];
        }

        public static Piece Get(string s)
        {
            switch (s)
            {
                case "Dead":
                    return _dead;
                case "AliveNoPlayer":
                    return _livePieces[0];
                case "AlivePlayer1":
                    return _livePieces[1];
                case "AlivePlayer2":
                    return _livePieces[2];
                default:
                    throw new ArgumentException($"Invalid piece name: {s}");
            }
        }

    }

    public class Dead : Piece
    {
        public override PieceName Name => PieceName.Dead;

        public override int Value => 0;

        public override Owner Owner => Owner.None;
        public static Dead Get() => (Dead)Get(PieceName.Dead);
    }
    public abstract class Alive : Piece
    {
        public override PieceName Name => PieceName.Alive;
        public override int Value => 1;
        public static Alive Get() => (Alive)Get(PieceName.Alive);
    }
    public class AliveNoPlayer : Alive
    {
        public override Owner Owner => Owner.None;
    }
    public class AlivePlayer1 : Alive
    {
        public override Owner Owner => Owner.Player1;
    }
    public class AlivePlayer2 : Alive
    {
        public override Owner Owner => Owner.Player2;
    }
   }
