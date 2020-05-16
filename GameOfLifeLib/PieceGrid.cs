using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib
{
    /// <summary>
    /// Define a grid of points with pieces. Each point will have a piece, even if that piece is an open space.
    /// The grid is a square with sides of a given size. 
    /// The square contains a set of x,y coordinate points with the lower left corner (0, 0)
    public class PieceGrid
    {
        private Guid _id;
        public Guid Id { get => _id; }

        /// <summary>
        /// Create a new un-initialized PieceGrid
        /// </summary>
        public PieceGrid() { _id = Guid.NewGuid(); }

        /// <summary>
        /// Create a new un-initialized PieceGrid of a given size
        /// </summary>
        /// <param name="size">Size of the square grid</param>
        public PieceGrid(int size)
        {
            _id = Guid.NewGuid();
            Size = size;
            PointPieces = new PointSquareArray<Piece>(size);
        }

        /// <summary>
        /// Create a new, empty PieceGrid of a given size and initialize it with a set of amazons
        /// </summary>
        /// <param name="size">Size of the square grid</param>
        /// <param name="playerPieces">(Point, Amazon) dictionary to set up player pieces</param>
        public PieceGrid(int size, IList<Point> playerPieces)
        {
            _id = Guid.NewGuid();
            Size = size;
            PointPieces = new PointSquareArray<Piece>(size);
            Initialize(playerPieces);
        }

        /// <summary>
        /// Set up a PieceGrid with a completely defined set of points
        /// </summary>
        /// <param name="allPieces">(Point, Piece) dictionary to set up all pieces</param>
        /// <exception cref="ArgumentException">You must specify all points on the grid in the allPieces dictionary</exception>
        public void Initialize(IDictionary<Point, Piece> allPieces)
        {
            Point p;
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    p.X = x;
                    p.Y = y;
                    if (!allPieces.ContainsKey(p))
                    {
                        PointPieces.Add(p, Piece.Get(PieceName.Dead));
                    }
                    else
                    {
                        PointPieces.Add(p, allPieces[p]);
                    }
                }
            }
        }

        /// <summary>
        /// Set up an empty PieceGrid with live cells at the given points
        /// </summary>
        /// <param name="initialLiveCells">point list to set up initital live cells</param>
        public void Initialize(IList<Point> initialLiveCells)
        {
            Point p;
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    p.X = x;
                    p.Y = y;
                    if (initialLiveCells.Contains(p))
                    {
                        PointPieces.Add(p, Piece.Get(PieceName.Alive));
                    }
                    else
                        PointPieces.Add(p, Piece.Get(PieceName.Dead));
                }
            }
        }

        /// <summary>
        /// Set up an empty PieceGrid with specific pieces at the given points
        /// </summary>
        /// <param name="initialLiveCells">point list to set up initital live cells</param>
        /// <param name="pieceName">name of piece to place</param>
        /// <param name="owner">owner of piece to place</param>
        public void Initialize(IList<Point> initialLiveCells, PieceName pieceName, Owner owner)
        {
            Point p;
            var piece = Piece.Get(pieceName, owner);
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    p.X = x;
                    p.Y = y;
                    if (initialLiveCells.Contains(p))
                    {
                        PointPieces.Add(p, piece);
                    }
                    else
                        PointPieces.Add(p, Piece.Get(PieceName.Dead));
                }
            }
        }

        /// <summary>
        /// Size of the square grid
        /// </summary>
        public int Size;
        /// <summary>
        /// All points and pieces on the grid
        /// </summary>
        public PointSquareArray<Piece> PointPieces;

        /// <summary>
        /// Check if a point is off the grid
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <returns>True if the given point is off the grid, false otherwise</returns>
        public bool IsOutOfBounds(Point point) => point.X < 0 || point.X >= Size || point.Y < 0 || point.Y >= Size;

        /// <summary>
        /// Make a deep clone of this PieceGrid
        /// </summary>
        /// <returns>Cloned PieceGrid</returns>
        public PieceGrid Clone()
        {
            PieceGrid newGrid = new PieceGrid(Size);
            newGrid.Initialize(PointPieces);
            return newGrid;
        }

    }
}
