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
        // the index number is for ranking the rules from most important (0) to least (n where max(n) is currently 7)
        static List<ICARule> AllRulesRank = new List<ICARule>();
        static Dictionary<ICARule, int> RulesRankDictionary;

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
            };

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
            Random random = new Random();
            Dictionary<Point, ICARule> rulePoints;
            Dictionary<Point, int> rulePointsAge;
            PieceGrid currentGen;
            // this functions sets up the intitial game
            //SeedsLifeJustFriendsRandomGame(random, out rulePoints, out rulePointsAge, out currentGen);
            SeedsSpreaderGame(random, out rulePoints, out rulePointsAge, out currentGen);
            //OtherSpreaderGame(random, out rulePoints, out rulePointsAge, out currentGen);
            
            Render(currentGen, rulePoints, windowPtr, screenSurface);

            bool quit = false;
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
                            currentGen.PointPieces[new Point(xIndex, yIndex)] = Piece.Get(1);
                            rulePoints[new Point(xIndex, yIndex)] = AllRulesRank[0];
                        }
                        else if (sdlEvent.button.button == SDL.SDL_BUTTON_RIGHT)
                        {
                            currentGen.PointPieces[new Point(xIndex, yIndex)] = Piece.Get(0);
                            rulePoints[new Point(xIndex, yIndex)] = AllRulesRank[0];
                        }
                        Render(currentGen, rulePoints, windowPtr, screenSurface);
                    }
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
                        rule = MajorityInRuleOrderChooser(random, rulePoints, nPoints);
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

        public delegate void GameSetup(Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen);
        private static unsafe GameSetup SeedsSpreaderGame = new GameSetup((Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen) =>
        {
            AllRulesRank.Add(RuleFactory.GetRuleByName("Life"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("JustFriends"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Pilot"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Seeds"));
            //AllRulesRank.Add(RuleFactory.GetRuleByName("HistoricalLife"));
            int index = 0;
            RulesRankDictionary = AllRulesRank.ToDictionary(r => r, r => index++);

            index = 0;
            foreach (ICARule rule in Program.AllRulesRank)
            {
                RuleBackgroundIndexes.Add(rule, index++);
            }

            rulePoints = new Dictionary<Point, ICARule>();
            rulePointsAge = new Dictionary<Point, int>();
            currentGen = new PieceGrid(100);
            currentGen.Initialize();
            CAPattern pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/JustFriendsBox50.rle");
            Point insertPoint = new Point(25, 25);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            insertPoint = new Point(35, 35);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsSmall.rle");
            insertPoint = new Point(2, 2);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PilotSmall.rle");
            insertPoint = new Point(30, 58);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);
        });

        private static unsafe GameSetup OtherSpreaderGame = new GameSetup((Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen) =>
        {
            AllRulesRank.Add(RuleFactory.GetRuleByName("JustFriends"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Life"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Pilot"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Seeds"));
            //AllRulesRank.Add(RuleFactory.GetRuleByName("HistoricalLife"));
            int index = 0;
            RulesRankDictionary = AllRulesRank.ToDictionary(r => r, r => index++);

            index = 0;
            foreach (ICARule rule in Program.AllRulesRank)
            {
                RuleBackgroundIndexes.Add(rule, index++);
            }

            rulePoints = new Dictionary<Point, ICARule>();
            rulePointsAge = new Dictionary<Point, int>();
            currentGen = new PieceGrid(100);
            currentGen.Initialize();
            CAPattern pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/JustFriendsBox50.rle");
            Point insertPoint = new Point(25, 25);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            insertPoint = new Point(35, 35);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsSmall.rle");
            insertPoint = new Point(2, 2);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PilotSmall.rle");
            insertPoint = new Point(30, 58);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);
        });

        private static unsafe GameSetup SeedsLifeJustFriendsRandomGame = new GameSetup((Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen) =>
        {
            AllRulesRank.Add(RuleFactory.GetRuleByName("Life"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("JustFriends"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Pilot"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Seeds"));
            //AllRulesRank.Add(RuleFactory.GetRuleByName("HistoricalLife"));
            int index = 0;
            RulesRankDictionary = AllRulesRank.ToDictionary(r => r, r => index++);

            index = 0;
            foreach (ICARule rule in Program.AllRulesRank)
            {
                RuleBackgroundIndexes.Add(rule, index++);
            }

            rulePoints = new Dictionary<Point, ICARule>();
            rulePointsAge = new Dictionary<Point, int>();
            currentGen = new PieceGrid(100);
            currentGen.Initialize();

            foreach(var kvp in currentGen.PointPieces.ToArray())
            {
                ICARule rule = AllRulesRank[random.Next(0, AllRulesRank.Count - 1)];
                int stateValue = random.Next(0, rule.NumStates);
                currentGen.PointPieces[kvp.Key] = Piece.Get(stateValue);
                rulePoints[kvp.Key] = rule;
            }

        });

        private static unsafe void InsertPattern(Dictionary<Point, ICARule> rulePoints, PieceGrid currentGen, CAPattern pattern, Point insertPoint)
        {
            foreach (var kvp in pattern.Pattern.PointPieces)
            {
                Point p = insertPoint + kvp.Key;
                currentGen.PointPieces[p] = kvp.Value;
                rulePoints[p] = pattern.Rule;
                //rulePoints.Add(p, pattern.Rule);
            }
        }

        // Rank is decided by more influence. ties are broken by random choice
        private static unsafe RuleChooser MajorityRuleChooser = new RuleChooser((random, rulePoints, nPoints) =>
        {
           ICARule rule;
           Dictionary<ICARule, int> LocalRules = nPoints.Select(p => rulePoints.TryGetValue(p, out ICARule r) ? r : RuleFactory.DefaultRule)
                                            .GroupBy(r => r)
                                            .ToDictionary(g => g.Key, g => g.Count());
           LocalRules.Remove(RuleFactory.DefaultRule);
            if (!LocalRules.Any())
            {
                return AllRulesRank[random.Next(0, AllRulesRank.Count - 1)];
            }
            else if (LocalRules.Count == 1)
            {
                return LocalRules.First().Key;
            }

            List<ICARule> choices = LocalRules.Where(c => c.Value == LocalRules.Max(k => k.Value)).Select(r => r.Key).ToList();
            if (choices.Count == 1)
            {
                rule = choices.First();
            }
            else
            {
                rule = choices[random.Next(0, choices.Count - 1)];
            }
            return rule;
        });

        // ruleInfluenceValues[i,j] 
        // i = ranked criteria (e.g. number of states in Rule - 2)
        // j = influence on center cell (i.e. number of cells with this rule in the neighborhood - 1)
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

        // Rank is decided by more influence preferring rules in rank order. Rules with better rank need less influence to "win" the cell
        // Ties are broken by random choice, but ties are less likely than pure majority rule
        private static unsafe RuleChooser MajorityInRuleOrderChooser = new RuleChooser((random, rulePoints, nPoints) =>
        {
            ICARule rule;
            Dictionary<ICARule, int> LocalRules = nPoints.Select(p => rulePoints.TryGetValue(p, out ICARule r) ? r : RuleFactory.DefaultRule)
                                             .GroupBy(r => r)
                                             .ToDictionary(g => g.Key, g => g.Count());
            LocalRules.Remove(RuleFactory.DefaultRule);
            if (!LocalRules.Any())
            {
                return AllRulesRank[0];

            }
            else if (LocalRules.Count == 1)
            {
                return LocalRules.First().Key;
            }

            
            int ruleInfluenceMinRank = LocalRules.Min(k => _ruleInfluenceValues[RulesRankDictionary[k.Key], k.Value - 1]);
            List<ICARule> choices = LocalRules.Where(k => _ruleInfluenceValues[RulesRankDictionary[k.Key], k.Value - 1] == ruleInfluenceMinRank)
                                              .Select(r => r.Key)
                                              .ToList();
            if (choices.Count == 1)
            {
                rule = choices[0];
            }
            else
            {
                rule = choices[random.Next(0, choices.Count - 1)];
            }
            return rule;
        });

        // Rank is decided by more influence preferring rules with fewer states. Rules with fewer states need less influence to "win" the cell
        // Ties are broken by random choice, but ties are less likely than pure majority rule
        private static unsafe RuleChooser FewerStatesRuleChooser = new RuleChooser((random, rulePoints, nPoints) =>
        {
            ICARule rule;
            Dictionary<ICARule, int> LocalRules = nPoints.Select(p => rulePoints.TryGetValue(p, out ICARule r) ? r : RuleFactory.DefaultRule)
                                             .GroupBy(r => r)
                                             .ToDictionary(g => g.Key, g => g.Count());
            LocalRules.Remove(RuleFactory.DefaultRule);
            if (!LocalRules.Any())
            {
                return AllRulesRank[random.Next(0, AllRulesRank.Count - 1)];
            }
            else if (LocalRules.Count == 1)
            {
                return LocalRules.First().Key;
            }
            int ruleInfluenceMinRank = LocalRules.Min(k => _ruleInfluenceValues[k.Key.NumStates - 2, k.Value - 1]);
            List<ICARule> choices = LocalRules.Where(k => _ruleInfluenceValues[k.Key.NumStates - 2, k.Value - 1] == ruleInfluenceMinRank)
                                              .Select(r => r.Key)
                                              .ToList();
            if (choices.Count == 1)
            {
                rule = choices.First();
            }
            else
            {
                rule = choices[random.Next(0, choices.Count - 1)];
            }
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
                    SDL.SDL_BlitSurface((IntPtr)StateSurfaces[kvp.Value.StateValue], IntPtr.Zero, (IntPtr)screenSurface, ref rect);
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
