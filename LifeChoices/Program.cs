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

namespace LifeChoices
{
    class Program
    {
        static int MaxStates = 36; // these are arbitrary 
        static int MaxRules = 36; // these are arbitrary 
        static unsafe SDL.SDL_Surface* [] StateSurfaces = new SDL.SDL_Surface*[MaxStates];
        static unsafe SDL.SDL_Surface* [] RuleBackgroundSurfaces = new SDL.SDL_Surface*[MaxRules];
        static unsafe SDL.SDL_Surface* [] StateSurfacesMini = new SDL.SDL_Surface*[MaxStates];
        static unsafe SDL.SDL_Surface* [] RuleBackgroundSurfacesMini = new SDL.SDL_Surface*[MaxRules];
        static Dictionary<ICARule, int> RuleBackgroundIndexes = new Dictionary<ICARule, int>();
        // the index number is for ranking the rules from most important (0) to least (n where max(n) is currently 7)
        static List<ICARule> AllRulesRank = new List<ICARule>();
        static Dictionary<ICARule, int> RulesRankDictionary;
        static Dictionary<ICARule, ElementalCombo> RuleElements = new Dictionary<ICARule, ElementalCombo>();

        struct ElementalCombo
        {
            public ElementalCombo(Element element)
            {
                ElementCount = 1;
                Air = element == Element.Air;
                Earth = element == Element.Earth;
                Fire = element == Element.Fire;
                Water = element == Element.Water;
            }

            public ElementalCombo(params Element [] elements)
            {
                if (elements == null || !elements.Any()) throw new ArgumentException("Must specify elements", "elements");
                ElementCount = elements.Distinct().Count();
                Air = elements.Contains(Element.Air);
                Earth = elements.Contains(Element.Earth);
                Fire = elements.Contains(Element.Fire);
                Water = elements.Contains(Element.Water);
            }

            public bool Air { get; }
            public bool Earth { get; }
            public bool Fire { get; }
            public bool Water { get; }
            public int ElementCount { get; }
            public IEnumerable<Element> Elements
            {
                get
                {
                    if (Air) yield return Element.Air;
                    if (Earth) yield return Element.Earth;
                    if (Fire) yield return Element.Fire;
                    if (Water) yield return Element.Water;
                }
            }
        }

        static unsafe void Main(string[] args)
        {
            Random random = new Random();
            Dictionary<Point, ICARule> rulePoints;
            Dictionary<Point, int> rulePointsAge;
            PieceGrid currentGen;
            // this function sets up the initial game
            //RandomGame(random, out rulePoints, out rulePointsAge, out currentGen, out RuleChooser ruleChooser);
            //StrangeBlinker(random, out rulePoints, out rulePointsAge, out currentGen, out RuleChooser ruleChooser);
            //SeedsJustFriendsMix(random, out rulePoints, out rulePointsAge, out currentGen, out RuleChooser ruleChooser);
            //Coral(random, out rulePoints, out rulePointsAge, out currentGen, out RuleChooser ruleChooser);
            ElementalGame(random, out rulePoints, out rulePointsAge, out currentGen, out RuleChooser ruleChooser);
            //SerizawaPilotMix(random, out rulePoints, out rulePointsAge, out currentGen, out RuleChooser ruleChooser);
            //LifeGliderGunMix(random, out rulePoints, out rulePointsAge, out currentGen, out RuleChooser ruleChooser);
            //LifeMegaMix(random, out rulePoints, out rulePointsAge, out currentGen, out RuleChooser ruleChooser);


            IntPtr windowPtr;
            SDL.SDL_Surface* screenSurface;
            bool renderMini = false;
            InitSDL(currentGen, renderMini, out windowPtr, out screenSurface);
            CreateSpriteSurfaces(screenSurface);
            //RenderSprites();

            Render(currentGen, renderMini, rulePoints, windowPtr, screenSurface);

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
                            currentGen.PointPieces[new Point(xIndex, yIndex)] = Piece.Get(1);
                            rulePoints[new Point(xIndex, yIndex)] = AllRulesRank[0];
                        }
                        else if (sdlEvent.button.button == SDL.SDL_BUTTON_RIGHT)
                        {
                            currentGen.PointPieces[new Point(xIndex, yIndex)] = Piece.Get(0);
                            rulePoints[new Point(xIndex, yIndex)] = AllRulesRank[0];
                        }
                        Render(currentGen, renderMini, rulePoints, windowPtr, screenSurface);
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
                    }
                }

                PieceGrid nextGen = currentGen.Clone();
                Dictionary<Point, ICARule> nextRulePoints = new Dictionary<Point, ICARule>();

                foreach (var kvp in currentGen.PointPieces)
                {
                    bool existingRule = rulePoints.TryGetValue(kvp.Key, out ICARule rule);
                    List<Point> nPoints = PointHelpers.GetAdjacentPointsToroid(kvp.Key, currentGen, PointHelpers.NeighborhoodOrder.Moore).ToList();
                    bool aliveNeighbors = nPoints.Any(p => currentGen.PointPieces[p].StateValue > 0);
                    if (!existingRule && !aliveNeighbors) continue;

                    if (!existingRule)
                    {
                        rule = ruleChooser(random, rulePoints, nPoints);
                        nextRulePoints.Add(kvp.Key, rule);
                        rulePointsAge[kvp.Key] = 1;
                    }
                    else if (!aliveNeighbors)
                    {
                        rulePointsAge[kvp.Key] = 0;
                    }
                    else
                    {
                        nextRulePoints.Add(kvp.Key, rule);
                        if (rulePointsAge.ContainsKey(kvp.Key)) rulePointsAge[kvp.Key]++;
                        else rulePointsAge[kvp.Key] = 1;
                    }

                    nextGen.PointPieces[kvp.Key] = rule.Run(currentGen, kvp.Key);
                }


                rulePoints = nextRulePoints;
                currentGen = nextGen;
                Render(currentGen, renderMini, rulePoints, windowPtr, screenSurface);
                if (msDelayIndex > 0) System.Threading.Thread.Sleep(msDelayList[msDelayIndex]);
            }

            SDL.SDL_DestroyWindow(windowPtr);
            SDL.SDL_Quit();
        }

        private static void RenderSprites()
        {
            throw new NotImplementedException();
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

        private static void ElementalGame(Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen, out RuleChooser ruleChooser)
        {
            ruleChooser = ElementalRuleOrderChooser;
            //List<string> ruleNames = new List<string> { "B3/S45678", "JustFriends", "Life", "Seeds" };
            //List<string> ruleNames = new List<string> { "B3/S45678", "Life", "JustFriends", "Seeds" };
            //List<string> ruleNames = new List<string> { "B3/S45678", "Seeds", "Life", "JustFriends" };
            //List<string> ruleNames = new List<string> { "B3/S45678", "JustFriends", "Seeds", "Life" };
            List<string> ruleNames = new List<string> { "JustFriends", "B3/S45678", "Life", "Seeds" };
            foreach(string ruleName in ruleNames) AllRulesRank.Add(RuleFactory.GetRuleByName(ruleName));

            ICARule earth = AllRulesRank[0];
            ICARule air = AllRulesRank[1];
            ICARule water = AllRulesRank[2];
            ICARule fire = AllRulesRank[3];
            ICARule airFire = RuleFactory.GetStrengthenRule(air, fire);
            ICARule earthWater = RuleFactory.GetStrengthenRule(earth, water);
            ICARule airEarth = RuleFactory.GetWeakenRule(air, earth);
            ICARule fireWater = RuleFactory.GetWeakenRule(fire, water);

            AllRulesRank.Add(airFire);
            AllRulesRank.Add(earthWater);
            AllRulesRank.Add(airEarth);
            AllRulesRank.Add(fireWater);

            RuleElements.Add(earth, new ElementalCombo(Element.Earth));
            RuleElements.Add(air, new ElementalCombo(Element.Air));
            RuleElements.Add(water, new ElementalCombo(Element.Water));
            RuleElements.Add(fire, new ElementalCombo(Element.Fire));
            RuleElements.Add(airFire, new ElementalCombo(Element.Air, Element.Fire));
            RuleElements.Add(earthWater, new ElementalCombo(Element.Earth, Element.Water));
            RuleElements.Add(airEarth, new ElementalCombo(Element.Air, Element.Earth));
            RuleElements.Add(fireWater, new ElementalCombo(Element.Fire, Element.Water));


            int index = 0;
            RulesRankDictionary = new Dictionary<ICARule, int>();
            RulesRankDictionary.Add(RuleFactory.GetRuleByName("B3/S45678"), 3);
            RulesRankDictionary.Add(RuleFactory.GetRuleByName("JustFriends"), 0);
            RulesRankDictionary.Add(RuleFactory.GetRuleByName("Life"), 0);
            RulesRankDictionary.Add(RuleFactory.GetRuleByName("Seeds"), 2);
            RulesRankDictionary.Add(airFire, 6);
            RulesRankDictionary.Add(earthWater, 6);
            RulesRankDictionary.Add(airEarth, 6);
            RulesRankDictionary.Add(fireWater, 6);

            index = 0;
            foreach (ICARule rule in Program.AllRulesRank)
            {
                RuleBackgroundIndexes.Add(rule, index++);
            }

            rulePoints = new Dictionary<Point, ICARule>();
            rulePointsAge = new Dictionary<Point, int>();
            currentGen = new PieceGrid(100);
            currentGen.Initialize();

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/JustFriendsBox50.rle");
            insertPoint = new Point(25, 25);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/coral.rle");
            insertPoint = new Point(5, 5);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            //pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            //insertPoint = new Point(48, 31);
            //InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            insertPoint = new Point(95, 95);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsSmall.rle");
            insertPoint = new Point(90, 90);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);
        }

        private static void Coral(Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen, out RuleChooser ruleChooser)
        {
            ruleChooser = MajorityInRuleOrderChooser;
            AllRulesRank.Add(RuleFactory.GetRuleByName("B3/S45678"));
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

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/coral.rle");
            insertPoint = new Point(5, 5);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

        }

        private static void LifeMegaMix(Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen, out RuleChooser ruleChooser)
        {
            ruleChooser = MajorityInRuleOrderChooser;
            AllRulesRank.Add(RuleFactory.GetRuleByName("JustFriends"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Life"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("HighLife"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Seeds"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("B34568/S15678"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("B234"));
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

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PersianRugs.rle");
            insertPoint = new Point(5, 5);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SpiralGrowth.rle");
            insertPoint = new Point(85, 45);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);


            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/JustFriendsBox50.rle");
            insertPoint = new Point(25, 25);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            insertPoint = new Point(48, 31);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);


            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsSmall.rle");
            insertPoint = new Point(2, 2);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            //pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            //insertPoint = new Point(55, 25);
            //InsertPattern(rulePoints, currentGen, pattern, insertPoint);

        }

        private static void LifeGliderGunMix(Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen, out RuleChooser ruleChooser)
        {
            ruleChooser = MajorityInRuleOrderChooser;
            AllRulesRank.Add(RuleFactory.GetRuleByName("Life"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("JustFriends"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Pilot"));
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
            insertPoint = new Point(48, 31);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            //pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            //insertPoint = new Point(55, 25);
            //InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PilotSmall.rle");
            insertPoint = new Point(1, 1);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PilotSmall.rle");
            insertPoint = new Point(5, 5);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);
        }

        public delegate void GameSetup(Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen, out RuleChooser ruleChooser);
        private static unsafe GameSetup SeedsJustFriendsMix = new GameSetup((Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen, out RuleChooser ruleChooser) =>
        {
            ruleChooser = MajorityInRuleOrderChooser;
            AllRulesRank.Add(RuleFactory.GetRuleByName("JustFriends"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Pilot"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Seeds"));
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

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsSmall.rle");
            insertPoint = new Point(2, 2);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);
        });

        private static unsafe GameSetup SerizawaPilotMix = new GameSetup((Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen, out RuleChooser ruleChooser) =>
        {
            ruleChooser = MajorityInRuleOrderChooser;
            AllRulesRank.Add(RuleFactory.GetRuleByName("Serizawa"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Pilot"));
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

            CAPattern pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SerizawaGunship.rle");
            Point insertPoint = new Point(25, 25);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PilotSmall.rle");
            insertPoint = new Point(70, 70);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);
        });

        private static unsafe GameSetup StrangeBlinker = new GameSetup((Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen, out RuleChooser ruleChooser) =>
        {
            ruleChooser = MajorityInRuleOrderChooser;
            AllRulesRank.Add(RuleFactory.GetRuleByName("JustFriends"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Null"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Seeds"));
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

            CAPattern pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsJustFriendsStrangeBlinker.rle");
            Point insertPoint = new Point(25, 25);
            InsertPattern(rulePoints, currentGen, pattern, insertPoint);
            rulePoints[new Point(26, 26)] = RuleFactory.GetRuleByName("Seeds");
            rulePoints[new Point(27, 27)] = RuleFactory.GetRuleByName("Seeds");

        });

        private static unsafe GameSetup RandomGame = new GameSetup((Random random, out Dictionary<Point, ICARule> rulePoints, out Dictionary<Point, int> rulePointsAge, out PieceGrid currentGen, out RuleChooser ruleChooser) =>
        {
            ruleChooser = MajorityInRuleOrderChooser;
            AllRulesRank.Add(RuleFactory.GetRuleByName("Life"));
            //AllRulesRank.Add(RuleFactory.GetRuleByName("Null"));
            AllRulesRank.Add(RuleFactory.GetRuleByName("Pilot"));
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
                ICARule rule = AllRulesRank[random.Next(0, AllRulesRank.Count)];
                int stateValue = random.Next(0, rule.NumStates);
                currentGen.PointPieces[kvp.Key] = Piece.Get(stateValue);
                rulePoints[kvp.Key] = rule;
            }

        });

        private static unsafe void InsertPattern(Dictionary<Point, ICARule> rulePoints, PieceGrid currentGen, CAPattern pattern, Point insertPoint)
        {
            if (currentGen.IsOutOfBounds(insertPoint)) throw new ArgumentException($"Insert point {insertPoint} is outside given PieceGrid (size {currentGen.Size})");
            if (pattern.Pattern.Size > currentGen.Size) throw new ArgumentException($"Pattern too big for given PieceGrid: {currentGen.Size} < {pattern.Pattern.Size}");
            foreach (var kvp in pattern.Pattern.PointPieces)
            {
                IEnumerable<Point> nPoints = kvp.Key.GetAdjacentPointsNotOutOfBounds(pattern.Pattern, PointHelpers.NeighborhoodOrder.Moore);
                bool aliveNeighbors = nPoints.Any(pt => pattern.Pattern.PointPieces[pt].StateValue > 0);
                if (aliveNeighbors)
                {
                    Point p = insertPoint.AddPointsToroid(kvp.Key, currentGen);
                    currentGen.PointPieces[p] = kvp.Value;
                    rulePoints[p] = pattern.Rule;
                }
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
        // i = ranked criteria (e.g. index of rule in AllRulesRank list) OR (number of states in Rule - 2)
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


        // Elemental
        private static unsafe RuleChooser ElementalRuleOrderChooser = new RuleChooser((random, rulePoints, nPoints) =>
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

            ICARule air = RuleElements.FirstOrDefault(kvp => kvp.Value.Air && kvp.Value.ElementCount == 1).Key;
            ICARule earth = RuleElements.FirstOrDefault(kvp => kvp.Value.Earth && kvp.Value.ElementCount == 1).Key;
            ICARule fire = RuleElements.FirstOrDefault(kvp => kvp.Value.Fire && kvp.Value.ElementCount == 1).Key;
            ICARule water = RuleElements.FirstOrDefault(kvp => kvp.Value.Water && kvp.Value.ElementCount == 1).Key;
            ElementalCombo ec = new ElementalCombo(LocalRules.SelectMany(kvp => RuleElements[kvp.Key].Elements).ToArray());
            switch (ec.ElementCount)
            {
                case 2:
                    if (ec.Air && ec.Fire) return RuleFactory.GetStrengthenRule(air, fire);
                    if (ec.Earth && ec.Water) return RuleFactory.GetStrengthenRule(earth, water);
                    if (ec.Air && ec.Earth) return RuleFactory.GetWeakenRule(air, earth);
                    if (ec.Fire && ec.Water) return RuleFactory.GetWeakenRule(fire, water);
                    // others are Neutral
                    break;
                case 3:
                    if (ec.Air && ec.Fire && ec.Water) return air;
                    if (ec.Earth && ec.Fire && ec.Water) return earth;
                    if (ec.Air && ec.Earth && ec.Water) return water;
                    if (ec.Air && ec.Earth && ec.Fire) return fire;
                    break;
                case 4:
                    // Neutral
                    break;
                default:
                    throw new Exception("too many elements.");
            }

            // Neutral
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
                    SDL.SDL_BlitSurface((IntPtr)surfaces[kvp.Value.StateValue], IntPtr.Zero, (IntPtr)screenSurface, ref rect);
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
