using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;
using GameOfLifeLib;
using GameOfLifeLib.Rules;
using GameOfLifeLib.Parsers;

namespace LifeChoices
{
    class Program
    {
        static IntPtr aliveBmp;
        static IntPtr alive1Bmp;
        static IntPtr alive1PlayedBmp;
        static IntPtr alive2Bmp;
        static IntPtr alive2PlayedBmp;
        static IntPtr deadBmp;
        static IntPtr deadTweak1Bmp;
        static IntPtr deadTweak2Bmp;
        static unsafe void Main(string[] args)
        {
            IntPtr windowPtr;
            SDL.SDL_Surface* screenSurface;
            InitSDL(out windowPtr, out screenSurface);
            aliveBmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("Alive.bmp"), screenSurface->format, 0);
            alive1Bmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("Alive1.bmp"), screenSurface->format, 0);
            alive1PlayedBmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("Alive1Played.bmp"), screenSurface->format, 0);
            alive2Bmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("Alive2.bmp"), screenSurface->format, 0);
            alive2PlayedBmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("Alive2Played.bmp"), screenSurface->format, 0);
            deadBmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("Dead.bmp"), screenSurface->format, 0);
            deadTweak1Bmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("DeadTweak1.bmp"), screenSurface->format, 0);
            deadTweak2Bmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("DeadTweak2.bmp"), screenSurface->format, 0);

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
            currentGen.Initialize(initialLiveCells, 1, Owner.None);
            //ICARule rule = new HighLifeRule();
            //ICARule rule = new LifeRule();
            //ICARule rule = new LifeRuleRandomOne(1000);
            */

            //IDictionary<Point, Piece> initialCells = new Dictionary<Point, Piece>()
            //{
            //    // player 1 
            //    {  Point.Get(25, 25), Piece.Get(1, Owner.Player1) },
            //    {  Point.Get(26, 25), Piece.Get(1, Owner.Player1) },
            //    {  Point.Get(25, 26), Piece.Get(1, Owner.Player1) },
            //    {  Point.Get(24, 26), Piece.Get(1, Owner.Player1) },
            //    {  Point.Get(25, 27), Piece.Get(1, Owner.Player1) },
            //    //{  Point.Get(26, 27), Piece.Get(1, Owner.Player1) },
            //    // player 2 
            //    {  Point.Get(75, 75), Piece.Get(1, Owner.Player2) },
            //    {  Point.Get(76, 75), Piece.Get(1, Owner.Player2) },
            //    {  Point.Get(75, 76), Piece.Get(1, Owner.Player2) },
            //    {  Point.Get(74, 76), Piece.Get(1, Owner.Player2) },
            //    {  Point.Get(75, 77), Piece.Get(1, Owner.Player2) },
            //    //{  Point.Get(76, 77), Piece.Get(1, Owner.Player2) },
            //};
            IDictionary<Point, Piece> initialCells = new Dictionary<Point, Piece>()
            {
                // player 1 
                {  Point.Get(1, 1), Piece.Get(1, Owner.Player1) },
                {  Point.Get(2, 1), Piece.Get(1, Owner.Player1) },
                {  Point.Get(1, 2), Piece.Get(1, Owner.Player1) },
                {  Point.Get(0, 2), Piece.Get(1, Owner.Player1) },
                {  Point.Get(1, 3), Piece.Get(1, Owner.Player1) },
                {  Point.Get(25, 25), Piece.Get(1, Owner.Player1) },
                {  Point.Get(26, 25), Piece.Get(1, Owner.Player1) },
                {  Point.Get(25, 26), Piece.Get(1, Owner.Player1) },
                {  Point.Get(24, 26), Piece.Get(1, Owner.Player1) },
                {  Point.Get(25, 27), Piece.Get(1, Owner.Player1) },
            };
            PieceGrid currentGen = new PieceGrid(100);
            currentGen.Initialize(initialCells);
            //LifeRuleCenterTwo rule = new LifeRuleCenterTwo(int.MaxValue);
            //ICARule rule = RuleFactory.GetRuleFromFile("RuleFiles/HistoricalLife.table");
            ICARule rule = RuleFactory.GetRuleFromFile("RuleFiles/Life.table");
            //ICARule rule = new LifeRule();

            Render(currentGen, new HashSet<Point>(), new HashSet<Point>(), windowPtr, screenSurface);

            bool quit = false;
            while (!quit)
            {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event sdlEvent) != 0)
                {
                    if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT) quit = true;
                }
                PieceGrid nextGen = rule.Run(currentGen);
                currentGen = nextGen;
                //Render(currentGen, rule.tweakPoints1, rule.tweakPoints2, windowPtr, screenSurface);
                Render(currentGen, new HashSet<Point>(), new HashSet<Point>(), windowPtr, screenSurface);
            }

            SDL.SDL_DestroyWindow(windowPtr);
            SDL.SDL_Quit();
        }

        private static unsafe void Render(PieceGrid pieceGrid, HashSet<Point> tweakPoints1, HashSet<Point> tweakPoints2, IntPtr windowPtr, SDL.SDL_Surface* screenSurface)
        {
            SDL.SDL_Rect rect = new SDL.SDL_Rect();
            rect.h = 10;
            rect.w = 10;
            foreach (var kvp in pieceGrid.PointPieces)
            {
                rect.x = kvp.Key.X * 10;
                rect.y = kvp.Key.Y * 10;
                if (kvp.Value.StateValue == 1)
                {
                    switch (kvp.Value.Owner)
                    {
                        case Owner.None:
                            SDL.SDL_BlitSurface(aliveBmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                            break;
                        case Owner.Player1:
                            if (kvp.Value.Aspect == PieceAspect.Played)
                                SDL.SDL_BlitSurface(alive1PlayedBmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                            else
                                SDL.SDL_BlitSurface(alive1Bmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                            break;
                        case Owner.Player2:
                            if (kvp.Value.Aspect == PieceAspect.Played)
                                SDL.SDL_BlitSurface(alive2PlayedBmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                            else
                                SDL.SDL_BlitSurface(alive2Bmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                            break;
                    }
                }
                else
                {
                    if (tweakPoints1.Contains(kvp.Key))
                        SDL.SDL_BlitSurface(deadTweak1Bmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                    else if (tweakPoints2.Contains(kvp.Key))
                        SDL.SDL_BlitSurface(deadTweak2Bmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                    else
                        SDL.SDL_BlitSurface(deadBmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                }
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
