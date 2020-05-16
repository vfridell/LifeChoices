using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib
{
    public class Board
    {
        public PieceGrid PieceGrid { get; set; }

        public Board() { }
        public Board(int size)
        {
             PieceGrid = new PieceGrid(size);
        }
        public int Size => PieceGrid.Size;

        public Board Clone()
        {
            Board newBoard = new Board();
            newBoard.PieceGrid = PieceGrid.Clone();
            return newBoard;
        }

        public static Board ComputeFutureBoard(Board board)
        {
            Board newBoard = board.Clone();
            //newBoard.ApplyMove(move);
            return newBoard;
        }
    }
}
