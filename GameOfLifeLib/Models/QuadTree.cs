using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models
{
    public class QuadTree<T>
        where T : IHaveRectangle
    {
        private QuadTree() { }

        public QuadTreeNode<T> Root { get; set; }
        public static QuadTree<T> GetNew(Point origin, int squareSideLength, int smallestSquareArea)
        {
            QuadTree<T> q = new QuadTree<T>();
            Rect rootRect = new Rect(origin.X, origin.Y, squareSideLength, squareSideLength);
            q.Root = new QuadTreeNode<T>(rootRect, smallestSquareArea);
            return q;
        }

        public void Push(T obj)
        {
            Root.Push(obj);
        }
    }
}
