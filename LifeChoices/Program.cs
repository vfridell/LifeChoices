using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;
using GameOfLifeLib;
using GameOfLifeLib.Rules;

namespace LifeChoices
{
    class Program
    {
        static IntPtr aliveBmp;
        static IntPtr alive1Bmp;
        static IntPtr alive2Bmp;
        static IntPtr deadBmp;
        static unsafe void Main(string[] args)
        {
            IntPtr windowPtr;
            SDL.SDL_Surface* screenSurface;
            InitSDL(out windowPtr, out screenSurface);
            aliveBmp = SDL.SDL_LoadBMP("Alive.bmp");
            alive1Bmp = SDL.SDL_LoadBMP("Alive1.bmp");
            alive2Bmp = SDL.SDL_LoadBMP("Alive2.bmp");
            deadBmp = SDL.SDL_LoadBMP("Dead.bmp");

            /*
            IList<Point> initialLiveCells = new List<Point>()
            {
                // R-pentomino
                Point.Get(50, 50),
                Point.Get(51, 50),
                Point.Get(50, 51),
                Point.Get(49, 51),
                Point.Get(50, 52),
                // something else
                Point.Get(51, 52),
            };
            PieceGrid currentGen = new PieceGrid(100);
            currentGen.Initialize(initialLiveCells, PieceName.Alive, Owner.None);
            //ICARule rule = new HighLifeRule();
            //ICARule rule = new LifeRule();
            //ICARule rule = new LifeRuleRandomOne(1000);
            */

            IDictionary<Point, Piece> initialCells = new Dictionary<Point, Piece>()
            {
                // player 1 
                {  Point.Get(25, 25), Piece.Get(PieceName.Alive, Owner.Player1) },
                {  Point.Get(26, 25), Piece.Get(PieceName.Alive, Owner.Player1) },
                {  Point.Get(25, 26), Piece.Get(PieceName.Alive, Owner.Player1) },
                {  Point.Get(24, 26), Piece.Get(PieceName.Alive, Owner.Player1) },
                {  Point.Get(25, 27), Piece.Get(PieceName.Alive, Owner.Player1) },
                //{  Point.Get(26, 27), Piece.Get(PieceName.Alive, Owner.Player1) },
                // player 2 
                {  Point.Get(75, 75), Piece.Get(PieceName.Alive, Owner.Player2) },
                {  Point.Get(76, 75), Piece.Get(PieceName.Alive, Owner.Player2) },
                {  Point.Get(75, 76), Piece.Get(PieceName.Alive, Owner.Player2) },
                {  Point.Get(74, 76), Piece.Get(PieceName.Alive, Owner.Player2) },
                {  Point.Get(75, 77), Piece.Get(PieceName.Alive, Owner.Player2) },
                //{  Point.Get(76, 77), Piece.Get(PieceName.Alive, Owner.Player2) },
            };
            PieceGrid currentGen = new PieceGrid(100);
            currentGen.Initialize(initialCells);
            ICARule rule = new LifeRuleCenterTwo(int.MaxValue);

            Render(currentGen, windowPtr, screenSurface);

            bool quit = false;
            while (!quit)
            {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event sdlEvent) != 0)
                {
                    if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT) quit = true;
                }
                PieceGrid nextGen = rule.Run(currentGen);
                currentGen = nextGen;
                Render(currentGen, windowPtr, screenSurface);
            }

            SDL.SDL_DestroyWindow(windowPtr);
            SDL.SDL_Quit();
        }

        private static unsafe void Render(PieceGrid pieceGrid, IntPtr windowPtr, SDL.SDL_Surface* screenSurface)
        {
            SDL.SDL_Rect rect = new SDL.SDL_Rect();
            rect.h = 10;
            rect.w = 10;
            foreach (var kvp in pieceGrid.PointPieces)
            {
                rect.x = kvp.Key.X * 10;
                rect.y = kvp.Key.Y * 10;
                if (kvp.Value.Name == PieceName.Alive)
                {
                    switch(kvp.Value.Owner)
                    {
                        case Owner.None:
                            SDL.SDL_BlitSurface(aliveBmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                            break;
                        case Owner.Player1:
                            SDL.SDL_BlitSurface(alive1Bmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                            break;
                        case Owner.Player2:
                            SDL.SDL_BlitSurface(alive2Bmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                            break;
                    }
                }
                else SDL.SDL_BlitSurface(deadBmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
            }
            SDL.SDL_UpdateWindowSurface(windowPtr);
        }

        private static unsafe void InitSDL(out IntPtr windowPtr, out SDL.SDL_Surface* screenSurface)
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0) throw new Exception("Could not init SDL");
            windowPtr = SDL2.SDL.SDL_CreateWindow("SDL Test", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 1000, 1000, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (windowPtr == null) throw new Exception("Could not create SDL window");
            screenSurface = (SDL.SDL_Surface*)SDL.SDL_GetWindowSurface(windowPtr);
        }
    }
}
