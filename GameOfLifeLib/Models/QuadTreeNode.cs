using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models
{
    public class QuadTreeNode<T>
        where T : IHaveRectangle
    {
        public List<T> ObjectList = new List<T>();
        public QuadTreeNode<T>[] Children;
        private Rect RectangleArea { get; set; }

        public IEnumerable<CollisionPair> GetCollidingObjects()
        {
            if (Children != null)
            {
                return Children[0].GetCollidingObjects()
                        .Union(Children[1].GetCollidingObjects())
                        .Union(Children[2].GetCollidingObjects())
                        .Union(Children[3].GetCollidingObjects());
            }
            else
            {
                List<CollisionPair> collisions = new List<CollisionPair>();
                foreach (T obj in ObjectList)
                {
                    collisions.AddRange(ObjectList
                        .Where(sub => !sub.Equals(obj))
                        .Where(sub => obj.Rectangle.Overlaps(sub.Rectangle))
                        .Select(sub => new CollisionPair(obj, sub))
                    );
                }
                return collisions.Distinct();
            }

        }

        public IEnumerable<T> GetObjectsAt(Point point)
        {
            if (Children != null)
            {
                QuadTreeNode<T> node = Children.Single(n => n.RectangleArea.Contains(point));
                return node.GetObjectsAt(point);
            }
            else
            {
                return ObjectList.Where(o => o.Rectangle.Contains(point));
            }
        }

        public QuadTreeNode(Rect rect, int smallestSquareArea)
        {
            RectangleArea = rect;
            if (smallestSquareArea < RectangleArea.SmallestSide)
            {
                Children = new QuadTreeNode<T>[4];

                bool even = RectangleArea.Width % 2 == 0;
                int extra = 0;
                if (!even) extra = 1;
                int half1 = (RectangleArea.Width / 2) + extra;
                int half2 = RectangleArea.Width / 2;
                Rect upperLeft, upperRight, lowerLeft, lowerRight;

                upperLeft = new Rect(RectangleArea.X, RectangleArea.Y, half1, half1);
                upperRight = new Rect(RectangleArea.X + half1 + 1, RectangleArea.Y, half2, half1);
                lowerLeft = new Rect(RectangleArea.X, RectangleArea.Y + half1 + 1, half1, half2);
                lowerRight = new Rect(RectangleArea.X + half1 + 1, RectangleArea.Y + half1 + 1, half2, half2);
                Children[0] = new QuadTreeNode<T>(upperLeft, smallestSquareArea);
                Children[1] = new QuadTreeNode<T>(upperRight, smallestSquareArea);
                Children[2] = new QuadTreeNode<T>(lowerLeft, smallestSquareArea);
                Children[3] = new QuadTreeNode<T>(lowerRight, smallestSquareArea);
            }
        }

        public void Push(T obj)
        {
            if (obj.Rectangle.Overlaps(RectangleArea))
            {
                if (Children != null)
                {
                    Children[0].Push(obj);
                    Children[1].Push(obj);
                    Children[2].Push(obj);
                    Children[3].Push(obj);
                }
                else
                {
                    ObjectList.Add(obj);
                }
            }
        }

        public class CollisionPair
        {
            public CollisionPair(T obj1, T obj2)
            {
                Object1 = obj1;
                Object2 = obj2;
            }

            public T Object1 { get; protected set; }
            public T Object2 { get; protected set; }

            public override bool Equals(object obj)
            {
                CollisionPair other = obj as CollisionPair;
                if (null == other) return false;
                return Equals(other);
            }

            public bool Equals(CollisionPair other)
            {
                return (Object1.Equals(other.Object1) && Object2.Equals(other.Object2)) ||
                        (Object1.Equals(other.Object2) && Object2.Equals(other.Object1));
            }

            public override int GetHashCode()
            {
                int hash = GetType().GetHashCode();
                hash = (hash * 397) ^ (Object1.Rectangle.GetHashCode() + Object2.Rectangle.GetHashCode());
                return hash;
            }
        }
    }
}
