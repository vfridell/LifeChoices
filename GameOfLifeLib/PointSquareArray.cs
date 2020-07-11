using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib
{
    public class PointSquareArray<T> : IDictionary<Point, T>
    {
        private T[,] _pieces;
        private int _arraySize;

        public PointSquareArray(int size)
        {
            _arraySize = size;
            _pieces = new T[size, size];
        }

        public PointSquareArray(PointSquareArray<T> src)
        {
            _arraySize = src._arraySize;
            _pieces = new T[_arraySize, _arraySize];
            for (int x = 0; x < _arraySize; x++)
                for (int y = 0; y < _arraySize; y++)
                    _pieces[x, y] = src._pieces[x, y];
        }

        public T this[Point p] { get => _pieces[p.X, p.Y]; set => _pieces[p.X, p.Y] = value; }

        public ICollection<Point> Keys => throw new NotImplementedException();

        public ICollection<T> Values => throw new NotImplementedException();

        public int Count => _arraySize * _arraySize;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(Point key, T value) => _pieces[key.X, key.Y] = value;

        public void Add(KeyValuePair<Point, T> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _pieces.Initialize();
        }

        public bool Contains(KeyValuePair<Point, T> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(Point key) => key.X >= 0 && key.X < _arraySize && key.Y >= 0 && key.Y < _arraySize;

        public void CopyTo(KeyValuePair<Point, T>[] array, int arrayIndex)
        {
            int i = arrayIndex;
            for (int x = 0; x < _arraySize; x++)
            {
                for (int y = 0; y < _arraySize; y++)
                {
                    Point point = new Point(x,y);
                    array[i++] = new KeyValuePair<Point, T>(point, _pieces[x, y]);
                }
            }
        }

        public IEnumerator<KeyValuePair<Point, T>> GetEnumerator()
        {
            Point point = new Point();
            for (int x = 0; x < _arraySize; x++)
            {
                for (int y = 0; y < _arraySize; y++)
                {
                    point.X = x;
                    point.Y = y;
                    yield return new KeyValuePair<Point, T>(point, _pieces[x, y]);
                }
            }
        }


        public bool Remove(Point key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<Point, T> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(Point key, out T value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
