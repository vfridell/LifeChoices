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

namespace LifeChoices
{
    class Program
    {
        static IntPtr aliveBmp;
        static IntPtr deadBmp;
        static IntPtr deadTweak1Bmp;
        static int MaxStates = 36; // these are arbitrary 
        static int MaxRules = 36; // these are arbitrary 
        static unsafe SDL.SDL_Surface* [] StateSurfaces = new SDL.SDL_Surface*[MaxStates];
        static unsafe SDL.SDL_Surface* [] RuleBackgroundSurfaces = new SDL.SDL_Surface*[MaxRules];
        static Dictionary<ICARule, int> RuleBackgroundIndexes = new Dictionary<ICARule, int>();
        static Dictionary<ICARule, int> AllRules;

        static unsafe void Main(string[] args)
        {
            IntPtr windowPtr;
            SDL.SDL_Surface* screenSurface;
            InitSDL(out windowPtr, out screenSurface);
            aliveBmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("Alive.bmp"), screenSurface->format, 0);
            deadBmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("Dead.bmp"), screenSurface->format, 0);
            deadTweak1Bmp = SDL.SDL_ConvertSurface(SDL.SDL_LoadBMP("DeadTweak1.bmp"), screenSurface->format, 0);

            var rgbValues = new List<List<byte>>()
            {
                new List<byte>() { 0, 0, 255 },
                new List<byte>() { 0, 255, 255 },
                new List<byte>() { 255, 255, 0 },
                new List<byte>() { 255, 0, 255 },
                new List<byte>() { 255, 0, 0 },
                new List<byte>() { 0, 255, 0 },
            } ;

            int stateValue = 0;
            StateSurfaces[stateValue++] = (SDL.SDL_Surface*)deadBmp;
            StateSurfaces[stateValue++] = (SDL.SDL_Surface*)aliveBmp;

            foreach (List<byte> rgbList in rgbValues)
            {
                SDL.SDL_Surface* newStateSurface = (SDL.SDL_Surface*)SDL.SDL_ConvertSurface(aliveBmp, screenSurface->format, 0);
                ColorMod(newStateSurface, rgbList[0], rgbList[1], rgbList[2]);
                StateSurfaces[stateValue++] = newStateSurface;
            }

            int ruleIndex = 0;
            foreach (List<byte> rgbList in rgbValues)
            {
                SDL.SDL_Surface* newBgSurface = (SDL.SDL_Surface*)SDL.SDL_ConvertSurface(deadTweak1Bmp, screenSurface->format, 0);
                ColorMod(newBgSurface, rgbList[0], rgbList[1], rgbList[2]);
                RuleBackgroundSurfaces[ruleIndex++] = newBgSurface;
            }

            //LifeRuleCenterTwo rule = new LifeRuleCenterTwo(int.MaxValue);
            //ICARule rule2 = RuleFactory.GetRuleFromFile("RuleFiles/HistoricalLife.table");
            ICARule rule1 = RuleFactory.GetRuleByName("Life");
            //ICARule rule1 = RuleFactory.GetRuleByName("HistoricalLife");
            ICARule rule2 = RuleFactory.GetRuleByName("Pilot");
            ICARule rule3 = RuleFactory.GetRuleByName("JustFriends");
            ICARule rule4 = RuleFactory.GetRuleByName("Seeds");
            ICARule rule5 = RuleFactory.GetRuleByName("HistoricalLife");
            //ICARule rule = RuleFactory.GetRuleFromFile("RuleFiles/Life.table");
            // the number is for counting the surrounding cells with a given rule. AllRules is defaulted to 1 on each rule as an arbitrary equality
            AllRules = new Dictionary<ICARule, int>() { { rule1, 1 }, { rule2, 1 }, { rule3, 1}, { rule4, 1}, { rule5, 1} };
            int index = 0;
            foreach(ICARule rule in AllRules.Keys)
            {
                RuleBackgroundIndexes.Add(rule, index++);
            }

            Random random = new Random();

            Dictionary<Point, ICARule> rulePoints = new Dictionary<Point, ICARule>();
            Dictionary<Point, int> rulePointsAge = new Dictionary<Point, int>();

            PieceGrid currentGen = new PieceGrid(100);

            currentGen.Initialize();
            CAPattern pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/JustFriendsTest.rle");
            Point insertPoint = new Point(25, 50);
            foreach(var kvp in pattern.Pattern.PointPieces)
            {
                Point p = insertPoint + kvp.Key;
                currentGen.PointPieces[p] = kvp.Value;
                rulePoints.Add(p, pattern.Rule);
            }

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/LifeShip.rle");
            insertPoint = new Point(50, 25);
            foreach (var kvp in pattern.Pattern.PointPieces)
            {
                Point p = insertPoint + kvp.Key;
                currentGen.PointPieces[p] = kvp.Value;
                rulePoints.Add(p, pattern.Rule);
            }

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsSmall.rle");
            insertPoint = new Point(2, 2);
            foreach (var kvp in pattern.Pattern.PointPieces)
            {
                Point p = insertPoint + kvp.Key;
                currentGen.PointPieces[p] = kvp.Value;
                rulePoints.Add(p, pattern.Rule);
            }

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PilotSmall.rle");
            insertPoint = new Point(80, 80);
            foreach (var kvp in pattern.Pattern.PointPieces)
            {
                Point p = insertPoint + kvp.Key;
                currentGen.PointPieces[p] = kvp.Value;
                rulePoints.Add(p, pattern.Rule);
            }

            Render(currentGen, rulePoints, windowPtr, screenSurface);

            bool quit = false;
            while (!quit)
            {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event sdlEvent) != 0)
                {
                    if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT) quit = true;
                }

                PieceGrid nextGen = currentGen.Clone();
                foreach (var kvp in currentGen.PointPieces)
                {
                    bool existingRule = rulePoints.TryGetValue(kvp.Key, out ICARule rule);
                    List<Point> nPoints = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen, PointHelpers.NeighborhoodOrder.Moore).ToList();
                    bool aliveNeighbors = nPoints.Any(p => currentGen.PointPieces[p].StateValue > 0);
                    if (!existingRule && !aliveNeighbors) continue;

                    if (!existingRule)
                    {
                        rule = FewerStatesRule(random, rulePoints, nPoints);
                        rulePoints.Add(kvp.Key, rule);
                        rulePointsAge[kvp.Key] = 1;
                    }
                    else if (!aliveNeighbors)
                    {
                        rulePoints.Remove(kvp.Key);
                        rulePointsAge[kvp.Key] = 0;
                    }
                    else
                    {
                        if (rulePointsAge.ContainsKey(kvp.Key)) rulePointsAge[kvp.Key]++;
                        else rulePointsAge[kvp.Key] = 1;
                    }

                    nextGen.PointPieces[kvp.Key] = rule.Run(currentGen, kvp.Key);

                    //if(rulePointsAge[kvp.Key] > 10)
                    //{
                    //    rulePoints.Remove(kvp.Key);
                    //    rulePointsAge[kvp.Key] = 0;
                    //}

                    //if(innerRuleArea.Contains(kvp.Key))
                    //    nextGen.PointPieces[kvp.Key] = rule2.Run(currentGen, kvp.Key);
                    //else 
                    //    nextGen.PointPieces[kvp.Key] = rule1.Run(currentGen, kvp.Key);
                }

                currentGen = nextGen;
                Render(currentGen, rulePoints, windowPtr, screenSurface);
            }

            SDL.SDL_DestroyWindow(windowPtr);
            SDL.SDL_Quit();
        }

        private static unsafe RuleChooser MajorityRule = new RuleChooser((random, rulePoints, nPoints) =>
        {
           ICARule rule;
           Dictionary<ICARule, int> LocalRules = nPoints.Select(p => rulePoints.TryGetValue(p, out ICARule r) ? r : RuleFactory.DefaultRule)
                                            .GroupBy(r => r)
                                            .ToDictionary(g => g.Key, g => g.Count());
           LocalRules.Remove(RuleFactory.DefaultRule);
           if (!LocalRules.Any()) LocalRules = AllRules;
           else if (LocalRules.Count == 1) return LocalRules.First().Key;
           List<ICARule> choices = LocalRules.Where(c => c.Value == LocalRules.Max(k => k.Value)).Select(r => r.Key).ToList();
           if (choices.Count == 1)
               rule = choices.First();
           else
               rule = choices[random.Next(0, choices.Count - 1)];
           return rule;
        });

        static int[,] _ruleInfluenceValues = new int[7, 7] 
        { 
            {128, 64, 32, 16, 8, 4, 2 },
            {256, 128, 64, 32, 16, 8, 4 },
            {512, 256, 128, 64, 32, 16, 8 },
            {1024, 512, 256, 128, 64, 32, 16 },
            {2048, 1024, 512, 256, 128, 64, 32 },
            {4092, 2048, 1024, 512, 256, 128, 64 },
            {8184, 4092, 2048, 1024, 512, 256, 128 },
        };
        private static unsafe RuleChooser FewerStatesRule = new RuleChooser((random, rulePoints, nPoints) =>
        {
            ICARule rule;
            Dictionary<ICARule, int> LocalRules = nPoints.Select(p => rulePoints.TryGetValue(p, out ICARule r) ? r : RuleFactory.DefaultRule)
                                             .GroupBy(r => r)
                                             .ToDictionary(g => g.Key, g => g.Count());
            LocalRules.Remove(RuleFactory.DefaultRule);
            if (!LocalRules.Any()) LocalRules = AllRules;
            else if (LocalRules.Count == 1) return LocalRules.First().Key;
            int ruleInfluenceMinRank = LocalRules.Min(k => _ruleInfluenceValues[k.Key.NumStates - 2, k.Value - 1]);
            List<ICARule> choices = LocalRules.Where(k => _ruleInfluenceValues[k.Key.NumStates - 2, k.Value - 1] == ruleInfluenceMinRank)
                                              .Select(r => r.Key)
                                              .ToList();
            if (choices.Count == 1)
                rule = choices.First();
            else
                rule = choices[random.Next(0, choices.Count - 1)];
            return rule;
        });

        public delegate ICARule RuleChooser(Random random, Dictionary<Point, ICARule> rulePoints, List<Point> nPoints);

        private static unsafe void Render(PieceGrid pieceGrid, Dictionary<Point, ICARule> tweakPoints1, IntPtr windowPtr, SDL.SDL_Surface* screenSurface)
        {
            SDL.SDL_Rect rect = new SDL.SDL_Rect();
            rect.h = 10;
            rect.w = 10;
            foreach (var kvp in pieceGrid.PointPieces)
            {
                rect.x = kvp.Key.X * 10;
                rect.y = kvp.Key.Y * 10;
                if (kvp.Value.StateValue >= 1)
                {
                    //SDL.SDL_BlitSurface((IntPtr)StateSurfaces[kvp.Value.StateValue], IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                }
                else
                {
                    if (tweakPoints1.TryGetValue(kvp.Key, out ICARule rule))
                    {
                        SDL.SDL_BlitSurface((IntPtr)RuleBackgroundSurfaces[RuleBackgroundIndexes[rule]], IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                    }
                    else
                    {
                        SDL.SDL_BlitSurface(deadBmp, IntPtr.Zero, (IntPtr)screenSurface, ref rect);
                    }
                }
            }
            SDL.SDL_UpdateWindowSurface(windowPtr);
        }

        private static unsafe void ColorMod(SDL.SDL_Surface* surface, byte r1, byte g1, byte b1)
        {
            int result = SDL.SDL_SetSurfaceColorMod((IntPtr)surface, r1, g1, b1);
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
