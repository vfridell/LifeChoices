using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;
using GameOfLifeLib;
using GameOfLifeLib.Rules;
using GameOfLifeLib.Parsers;
using GameOfLifeLib.Models;
using GameOfLifeLib.Helpers;
using GameOfLifeLib.Models.Games;
using GameOfLifeLib.Models.RuleChoosers;

namespace LifeChoices
{
    class Program
    {
        static int MaxStates = 11; // these are arbitrary 
        static int MaxRules = 9; // these are arbitrary 
        static unsafe SDL.SDL_Surface* [] StateSurfaces = new SDL.SDL_Surface*[MaxStates];
        static unsafe SDL.SDL_Surface* [] RuleBackgroundSurfaces = new SDL.SDL_Surface*[MaxRules];
        static unsafe SDL.SDL_Surface* [] StateSurfacesMini = new SDL.SDL_Surface*[MaxStates];
        static unsafe SDL.SDL_Surface* [] RuleBackgroundSurfacesMini = new SDL.SDL_Surface*[MaxRules];
        static Dictionary<ICARule, int> RuleBackgroundIndexes = new Dictionary<ICARule, int>();
        static bool rulesOnlyDisplay = false;

        static unsafe void Main(string[] args)
        {
            //Dictionary<string, int> RulesRankDictionary = new Dictionary<string, int>()
            //{
            //    // persian rugs
            //    { "B234" , 2 },
            //    // coral
            //    { "B3/S45678" , 0 },
            //};
            //Dictionary<string, int> RulesRankDictionary = new Dictionary<string, int>()
            //{
            //    //{ "Life" , 1 },
            //    { "Seeds" , 6 },
            //    //{ "JustFriends" , 1 },
            //    { "Serizawa" , 0 },
            //    //{ "B234" , 2 },
            //    //{ "B3/S45678" , 0 },
            //};
            //ToroidGameBase game = new RandomGame(RulesRankDictionary);

            //ToroidGameBase game = new CoralGame();
            //ToroidGameBase game = new SeedsJustFriendsMix();
            //ToroidGameBase game = new SerizawaPilotMix();
            //ToroidGameBase game = new LifeGliderGunMix();
            //ToroidGameBase game = new LifeMegaMix();
            ToroidGameBase game = new ElementalGame();

            //game.Initialize(60, new MajorityRuleChooser());
            game.Initialize();

            int index = 0;
            foreach (ICARule rule in game.AllRules)
            {
                RuleBackgroundIndexes.Add(rule, index++);
            }

            IntPtr windowPtr;
            SDL.SDL_Surface* screenSurface;
            bool renderMini = false;
            InitSDL(game.CurrentGeneration, renderMini, out windowPtr, out screenSurface);
            CreateSpriteSurfaces(screenSurface);
            RenderSprites(renderMini, windowPtr, screenSurface);

            Render(game.CurrentGeneration, renderMini, game.RulePoints, windowPtr, screenSurface);

            bool quit = false;
            List<int> msDelayList = new List<int>() { 0, 20, 50, 100, 300, 500, 1000, 2000 };
            int msDelayIndex = 0;
            while (!quit)
            {

                while (SDL.SDL_PollEvent(out SDL.SDL_Event sdlEvent) != 0)
                {
                    if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT) quit = true;
                    else if (sdlEvent.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP)
                    {
                        int xIndex = (int)Math.Floor(sdlEvent.button.x / 10d);
                        int yIndex = (int)Math.Floor(sdlEvent.button.y / 10d);
                        if (sdlEvent.button.button == SDL.SDL_BUTTON_LEFT)
                        {
                            //currentGen.PointPieces[new Point(xIndex, yIndex)] = Piece.Get(1);
                            //rulePoints[new Point(xIndex, yIndex)] = AllRulesRank[0];
                        }
                        else if (sdlEvent.button.button == SDL.SDL_BUTTON_RIGHT)
                        {
                            //currentGen.PointPieces[new Point(xIndex, yIndex)] = Piece.Get(0);
                            //rulePoints[new Point(xIndex, yIndex)] = AllRulesRank[0];
                        }
                        Render(game.CurrentGeneration, renderMini, game.RulePoints, windowPtr, screenSurface);
                    }
                    else if (sdlEvent.type == SDL.SDL_EventType.SDL_KEYUP)
                    {
                        if ((sdlEvent.key.keysym.sym == SDL.SDL_Keycode.SDLK_KP_MINUS || sdlEvent.key.keysym.sym == SDL.SDL_Keycode.SDLK_MINUS) && msDelayIndex < msDelayList.Count - 1)
                        {
                            msDelayIndex++;
                        }
                        else if ((sdlEvent.key.keysym.sym == SDL.SDL_Keycode.SDLK_KP_PLUS || sdlEvent.key.keysym.sym == SDL.SDL_Keycode.SDLK_PLUS) && msDelayIndex > 0)
                        {
                            msDelayIndex--;
                        }
                        else if ((sdlEvent.key.keysym.sym == SDL.SDL_Keycode.SDLK_r))
                        {
                            rulesOnlyDisplay = !rulesOnlyDisplay;
                        }
                    }
                }

                game.ExecuteGameLoop();

                Render(game.CurrentGeneration, renderMini, game.RulePoints, windowPtr, screenSurface);
                if (msDelayIndex > 0) System.Threading.Thread.Sleep(msDelayList[msDelayIndex]);
            }

            SDL.SDL_DestroyWindow(windowPtr);
            SDL.SDL_Quit();
        }

        private static unsafe void RenderSprites(bool mini, IntPtr windowPtr, SDL.SDL_Surface* screenSurface)
        {
            int size = 10;
            
            SDL.SDL_Surface*[] surfaces = StateSurfaces;
            SDL.SDL_Surface*[] bgSurfaces = RuleBackgroundSurfaces;
            if (mini)
            {
                size = 3;
                surfaces = StateSurfacesMini;
                bgSurfaces = RuleBackgroundSurfacesMini;
            }
            SDL.SDL_Rect rect = new SDL.SDL_Rect();
            rect.h = size;
            rect.w = size;
            var pieceGrid = new PieceGrid(100);
            int index = 0;
            foreach (var kvp in pieceGrid.PointPieces)
            {
                rect.x = kvp.Key.X * size;
                rect.y = kvp.Key.Y * size;
                if (index < surfaces.Length)
                {
                    SDL.SDL_BlitSurface((IntPtr)surfaces[index++], IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                }
                else if (index - surfaces.Length < bgSurfaces.Length)
                {
                    int convertedIndex = index - surfaces.Length;
                    index++;
                    SDL.SDL_BlitSurface((IntPtr)bgSurfaces[convertedIndex], IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                }
                else
                {
                    index = 0;
                }
            }
            SDL.SDL_UpdateWindowSurface(windowPtr);
        }

        private static unsafe void CreateSpriteSurfaces(SDL.SDL_Surface* screenSurface)
        {
            IntPtr aliveBmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("Alive.bmp"), screenSurface->format, 0);
            IntPtr deadBmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("Dead.bmp"), screenSurface->format, 0);
            IntPtr deadTweak1Bmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("DeadTweak1.bmp"), screenSurface->format, 0);
            IntPtr aliveMiniBmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("AliveMini.bmp"), screenSurface->format, 0);
            IntPtr deadMiniBmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("DeadMini.bmp"), screenSurface->format, 0);
            IntPtr deadMiniTweak1Bmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("DeadMiniTweak1.bmp"), screenSurface->format, 0);

            var rgbValues = new List<List<byte>>()
            {
                new List<byte>() { 0, 0, 255 },
                new List<byte>() { 0, 255, 255 },
                new List<byte>() { 255, 255, 0 },
                new List<byte>() { 255, 0, 255 },
                new List<byte>() { 255, 0, 0 },
                new List<byte>() { 0, 255, 0 },
                new List<byte>() { 64, 255, 64 },
                new List<byte>() { 255, 64, 64 },
                new List<byte>() { 64, 64, 255 },
            };

            int stateValue = 0;
            int stateValueMini = 0;
            StateSurfaces[stateValue++] = (SDL.SDL_Surface*)deadBmp;
            StateSurfaces[stateValue++] = (SDL.SDL_Surface*)aliveBmp;
            StateSurfacesMini[stateValueMini++] = (SDL.SDL_Surface*)deadMiniBmp;
            StateSurfacesMini[stateValueMini++] = (SDL.SDL_Surface*)aliveMiniBmp;

            foreach (List<byte> rgbList in rgbValues)
            {
                SDL.SDL_Surface* newStateSurface = (SDL.SDL_Surface*)SDL.SDL_ConvertSurface(aliveBmp, screenSurface->format, 0);
                ColorMod(newStateSurface, rgbList[0], rgbList[1], rgbList[2]);
                StateSurfaces[stateValue++] = newStateSurface;

                SDL.SDL_Surface* newStateSurfaceMini = (SDL.SDL_Surface*)SDL.SDL_ConvertSurface(aliveMiniBmp, screenSurface->format, 0);
                ColorMod(newStateSurfaceMini, rgbList[0], rgbList[1], rgbList[2]);
                StateSurfacesMini[stateValueMini++] = newStateSurfaceMini;
            }

            int ruleIndex = 0;
            int ruleIndexMini = 0;
            foreach (List<byte> rgbList in rgbValues)
            {
                SDL.SDL_Surface* newBgSurface = (SDL.SDL_Surface*)SDL.SDL_ConvertSurface(deadTweak1Bmp, screenSurface->format, 0);
                ColorMod(newBgSurface, rgbList[0], rgbList[1], rgbList[2]);
                RuleBackgroundSurfaces[ruleIndex++] = newBgSurface;

                SDL.SDL_Surface* newBgSurfaceMini = (SDL.SDL_Surface*)SDL.SDL_ConvertSurface(deadMiniTweak1Bmp, screenSurface->format, 0);
                ColorMod(newBgSurfaceMini, rgbList[0], rgbList[1], rgbList[2]);
                RuleBackgroundSurfacesMini[ruleIndexMini++] = newBgSurfaceMini;
            }
        }

        private static unsafe void Render(PieceGrid pieceGrid, bool mini, Dictionary<Point, ICARule> tweakPoints1, IntPtr windowPtr, SDL.SDL_Surface* screenSurface)
        {
            int size = 10;
            SDL.SDL_Surface*[] surfaces = StateSurfaces;
            SDL.SDL_Surface*[] bgSurfaces = RuleBackgroundSurfaces;
            if (mini)
            {
                size = 3;
                 surfaces = StateSurfacesMini;
                 bgSurfaces = RuleBackgroundSurfacesMini;
            }
            SDL.SDL_Rect rect = new SDL.SDL_Rect();
            rect.h = size;
            rect.w = size;
            foreach (var kvp in pieceGrid.PointPieces)
            {
                rect.x = kvp.Key.X * size;
                rect.y = kvp.Key.Y * size;
                if (kvp.Value.StateValue >= 1)
                {
                    if(!rulesOnlyDisplay) SDL.SDL_BlitSurface((IntPtr)surfaces[kvp.Value.StateValue], IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                }
                else
                {
                    if (tweakPoints1.TryGetValue(kvp.Key, out ICARule rule))
                    {
                        SDL.SDL_BlitSurface((IntPtr)bgSurfaces[RuleBackgroundIndexes[rule]], IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                    }
                    else
                    {
                        SDL.SDL_BlitSurface((IntPtr)surfaces[0], IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                    }
                }
            }
            SDL.SDL_UpdateWindowSurface(windowPtr);
        }

        private static unsafe void ColorMod(SDL.SDL_Surface* surface, byte r1, byte g1, byte b1)
        {
            int result = SDL.SDL_SetSurfaceColorMod((IntPtr)surface, r1, g1, b1);
        }

        private static unsafe void InitSDL(PieceGrid grid, bool mini, out IntPtr windowPtr, out SDL.SDL_Surface* screenSurface)
        {

            int x;
            int y;
            int factor = 10;
            if (mini) factor = 3;
            x = grid.Size * factor;
            y = grid.Size * factor;
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0) throw new Exception("Could not init SDL");
            windowPtr = SDL2.SDL.SDL_CreateWindow("SDL Test", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, x, y, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (windowPtr == null) throw new Exception("Could not create SDL window");
            screenSurface = (SDL.SDL_Surface*)SDL.SDL_GetWindowSurface(windowPtr);
        }
    }
}
