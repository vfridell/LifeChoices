using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GridDirection = GameOfLifeLib.PointHelpers.GridDirection;

namespace GameOfLifeLib.Rules
{
    public class NonIsoTotalisticRule : ICARule
    {
        public List<int> BirthNeighborCounts { get; protected set; }
        public List<int> SurvivalNeighborCounts { get; protected set; }
        public Dictionary<int, string> NonTotalBirthRestrictions { get; protected set; }
        public Dictionary<int, string> NonTotalSurvivalRestrictions { get; protected set; }
        public int NumStates => 2;
        public string Name { get; protected set; }

        private static Regex _ruleStringRegex = new Regex("^B((?:[0-8]-?[cekainyqjrtwz]*)+)/S((?:[0-8]-?[cekainyqjrtwz]*)+)$|^B((?:[0-8]-?[cekainyqjrtwz]*)+)$");
        private static Regex _subRuleStringRegex = new Regex("([0-8])(-?[cekainyqjrtwz]*)");
        private static Dictionary<char, List<List<GridDirection>>> _restrictionsBaseDictionary;
        private static Dictionary<char, List<List<HashSet<int>>>> _restrictionPointsDictionary;



        public NonIsoTotalisticRule(string name, string ruleString)
        {
            Name = name;
            BirthNeighborCounts = new List<int>();
            SurvivalNeighborCounts = new List<int>();
            NonTotalBirthRestrictions = new Dictionary<int, string>();
            NonTotalSurvivalRestrictions = new Dictionary<int, string>();
            ParseRuleStringRegex(ruleString);
        }

        private void ParseRuleStringRegex(string ruleString)
        {
            var matchColl = _ruleStringRegex.Matches(ruleString);

            //births (with survival)
            if (_subRuleStringRegex.IsMatch(matchColl[0].Groups[1].Value))
            {
                MatchCollection matches = _subRuleStringRegex.Matches(matchColl[0].Groups[1].Value);
                foreach(Match match in matches)
                {
                    int num = int.Parse(match.Groups[1].Value);
                    BirthNeighborCounts.Add(num);
                    string config = match.Groups[2].Value;
                    if (!string.IsNullOrEmpty(config))
                    {
                        NonTotalBirthRestrictions.Add(num, config);
                    }
                }
            }

            //survival (with births)
            if (_subRuleStringRegex.IsMatch(matchColl[0].Groups[2].Value))
            {
                MatchCollection matches = _subRuleStringRegex.Matches(matchColl[0].Groups[2].Value);
                foreach (Match match in matches)
                {
                    int num = int.Parse(match.Groups[1].Value);
                    SurvivalNeighborCounts.Add(num);
                    string config = match.Groups[2].Value;
                    if (!string.IsNullOrEmpty(config))
                    {
                        NonTotalSurvivalRestrictions.Add(num, config);
                    }
                }
            }

            // births (no survival)
            if (_subRuleStringRegex.IsMatch(matchColl[0].Groups[3].Value))
            {
                MatchCollection matches = _subRuleStringRegex.Matches(matchColl[0].Groups[3].Value);
                foreach (Match match in matches)
                {
                    int num = int.Parse(match.Groups[1].Value);
                    BirthNeighborCounts.Add(num);
                    string config = match.Groups[2].Value;
                    if (!string.IsNullOrEmpty(config))
                    {
                        NonTotalBirthRestrictions.Add(num, config);
                    }
                }
            }
        }

        public static bool LifeLikeRuleString(string ruleString) => _ruleStringRegex.IsMatch(ruleString);

        public PieceGrid Run(PieceGrid currentGen)
        {
            PieceGrid nextGen = currentGen.Clone();
            foreach (var kvp in currentGen.PointPieces)
            {
                nextGen.PointPieces[kvp.Key] = Run(currentGen, kvp.Key, kvp.Value);
            }
            return nextGen;
        }


        public Piece Run(PieceGrid currentGen, Point point) => Run(currentGen, point, currentGen.PointPieces[point]);


        public Piece Run(PieceGrid currentGen, Point point, Piece piece)
        {
            var pts = PointHelpers.GetAdjacentPointsToroid(point, currentGen, PointHelpers.NeighborhoodOrder.Moore).ToList();
            int aliveNeighbors = pts.Count(p => currentGen.PointPieces[p].StateValue > 0);
            switch (piece.StateValue)
            {
                case 0:
                    if (BirthNeighborCounts.Contains(aliveNeighbors))
                    {
                        if (NonTotalBirthRestrictions.TryGetValue(aliveNeighbors, out string restriction))
                        {
                            bool negative = restriction[0] == '-';
                            string restrictCode;
                            if (negative) restrictCode = restriction.Substring(1);
                            else restrictCode = restriction;

                            foreach (char c in restrictCode)
                            {
                                foreach (HashSet<int> restrictedPoints in _restrictionPointsDictionary[c][aliveNeighbors])
                                {
                                    if (restrictedPoints.All(p => currentGen.PointPieces[pts[p]].StateValue != 0))
                                    {
                                        if (!negative) return Piece.Get(1);
                                        else return Piece.Get(0);
                                    }
                                }
                            }
                            if (negative) return Piece.Get(1);
                            else return Piece.Get(0);
                        }
                        else
                        {
                            return Piece.Get(1);
                        }
                    }
                    else return Piece.Get(0);

                default:
                case 1:
                    if (SurvivalNeighborCounts.Contains(aliveNeighbors))
                    {
                        if (NonTotalSurvivalRestrictions.TryGetValue(aliveNeighbors, out string restriction))
                        {
                            bool negative = restriction[0] == '-';
                            string restrictCode;
                            if (negative) restrictCode = restriction.Substring(1);
                            else restrictCode = restriction;

                            foreach (char c in restrictCode)
                            {
                                foreach (HashSet<int> restrictedPoints in _restrictionPointsDictionary[c][aliveNeighbors])
                                {
                                    if (restrictedPoints.All(p => currentGen.PointPieces[pts[p]].StateValue != 0))
                                    {
                                        if (!negative) return Piece.Get(1);
                                        else return Piece.Get(0);
                                    }
                                }
                            }
                            if (negative) return Piece.Get(1);
                            else return Piece.Get(0);
                        }
                        else
                        {
                            return Piece.Get(1);
                        }
                    }
                    else return Piece.Get(0);
            }
        }

        // these only work for NeighborhoodOrder.Moore and NeighborhoodOrder.MooreRuleTree
        // 0|4|1
        // _____
        // 5|x|6
        // -----
        // 2|7|3
        static NonIsoTotalisticRule()
        {
            _restrictionsBaseDictionary = new Dictionary<char, List<List<GridDirection>>>()
            {
                { 'c', new List<List<GridDirection>>()
                    {
                        { null },
                        new List<GridDirection> { GridDirection.NorthWest },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.NorthEast },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.NorthEast, GridDirection.SouthEast },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.NorthEast, GridDirection.SouthEast, GridDirection.SouthWest },
                        new List<GridDirection> { GridDirection.North, GridDirection.East, GridDirection.West, GridDirection.South, GridDirection.SouthWest },
                        new List<GridDirection> { GridDirection.North, GridDirection.East, GridDirection.West, GridDirection.South, GridDirection.SouthWest, GridDirection.SouthEast },
                        new List<GridDirection> { GridDirection.North, GridDirection.East, GridDirection.West, GridDirection.South, GridDirection.SouthWest, GridDirection.SouthEast, GridDirection.NorthEast },
                        { null },
                    }
                },
                { 'e', new List<List<GridDirection>>()
                    {
                        { null },
                        new List<GridDirection> { GridDirection.North},
                        new List<GridDirection> { GridDirection.North, GridDirection.West },
                        new List<GridDirection> { GridDirection.North, GridDirection.West, GridDirection.East },
                        new List<GridDirection> { GridDirection.North, GridDirection.West, GridDirection.East, GridDirection.South },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.NorthEast, GridDirection.SouthEast, GridDirection.SouthWest, GridDirection.South },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.NorthEast, GridDirection.SouthEast, GridDirection.SouthWest, GridDirection.South, GridDirection.East },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.NorthEast, GridDirection.SouthEast, GridDirection.SouthWest, GridDirection.South, GridDirection.East, GridDirection.West },
                        { null },
                    }
                },
                { 'k', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.North, GridDirection.SouthEast },
                        new List<GridDirection> { GridDirection.North, GridDirection.SouthEast, GridDirection.West },
                        new List<GridDirection> { GridDirection.North, GridDirection.SouthEast, GridDirection.West, GridDirection.NorthEast },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.NorthEast, GridDirection.East, GridDirection.South, GridDirection.SouthWest },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.NorthEast, GridDirection.East, GridDirection.South, GridDirection.SouthWest, GridDirection.West },
                        { null },
                        { null },
                    }
                },
                { 'a', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.North, GridDirection.NorthWest },
                        new List<GridDirection> { GridDirection.North, GridDirection.NorthWest, GridDirection.West },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.West, GridDirection.SouthWest, GridDirection.South },
                        new List<GridDirection> { GridDirection.SouthWest, GridDirection.South, GridDirection.SouthEast, GridDirection.East, GridDirection.NorthEast },
                        new List<GridDirection> { GridDirection.SouthWest, GridDirection.South, GridDirection.SouthEast, GridDirection.East, GridDirection.NorthEast, GridDirection.West },
                        { null },
                        { null },
                    }
                },
                { 'i', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.North, GridDirection.South },
                        new List<GridDirection> { GridDirection.West, GridDirection.NorthWest, GridDirection.SouthWest },
                        new List<GridDirection> { GridDirection.West, GridDirection.NorthWest, GridDirection.East, GridDirection.NorthEast },
                        new List<GridDirection> { GridDirection.North, GridDirection.NorthEast, GridDirection.East, GridDirection.SouthEast, GridDirection.South },
                        new List<GridDirection> { GridDirection.West, GridDirection.NorthWest, GridDirection.SouthWest, GridDirection.East, GridDirection.NorthEast, GridDirection.SouthEast },
                        { null },
                        { null },
                    }
                },
                { 'n', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.SouthEast },
                        new List<GridDirection> { GridDirection.West, GridDirection.NorthWest, GridDirection.NorthEast },
                        new List<GridDirection> { GridDirection.West, GridDirection.NorthWest, GridDirection.SouthWest, GridDirection.SouthEast },
                        new List<GridDirection> { GridDirection.SouthWest, GridDirection.South, GridDirection.SouthEast, GridDirection.East, GridDirection.North },
                        new List<GridDirection> { GridDirection.West, GridDirection.SouthWest, GridDirection.South, GridDirection.North, GridDirection.NorthEast, GridDirection.East },
                        { null },
                        { null },
                    }
                },
                { 'y', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.South, GridDirection.NorthWest, GridDirection.NorthEast },
                        new List<GridDirection> { GridDirection.South, GridDirection.NorthWest, GridDirection.NorthEast, GridDirection.SouthWest },
                        new List<GridDirection> { GridDirection.North, GridDirection.East, GridDirection.West, GridDirection.SouthEast, GridDirection.SouthWest },
                        { null },
                        { null },
                        { null },
                    }
                },
                { 'q', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.West, GridDirection.SouthEast },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.West, GridDirection.SouthEast, GridDirection.North },
                        new List<GridDirection> { GridDirection.North, GridDirection.NorthEast, GridDirection.East, GridDirection.South, GridDirection.SouthWest },
                        { null },
                        { null },
                        { null },
                    }
                },
                { 'j', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.NorthEast, GridDirection.East, GridDirection.South },
                        new List<GridDirection> { GridDirection.NorthEast, GridDirection.East, GridDirection.South, GridDirection.West },
                        new List<GridDirection> { GridDirection.North, GridDirection.NorthWest, GridDirection.West, GridDirection.SouthWest, GridDirection.SouthEast },
                        { null },
                        { null },
                        { null },
                    }
                },
                { 'r', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.North, GridDirection.NorthEast, GridDirection.South },
                        new List<GridDirection> { GridDirection.North, GridDirection.NorthEast, GridDirection.South, GridDirection.East },
                        new List<GridDirection> { GridDirection.West, GridDirection.NorthWest, GridDirection.SouthWest, GridDirection.East, GridDirection.SouthEast },
                        { null },
                        { null },
                        { null },
                    }
                },
                { 't', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.North, GridDirection.NorthEast, GridDirection.South, GridDirection.NorthWest },
                        { null },
                        { null },
                        { null },
                        { null },
                    }
                },
                { 'w', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.West, GridDirection.South, GridDirection.SouthEast },
                        { null },
                        { null },
                        { null },
                        { null },
                    }
                },
                { 'z', new List<List<GridDirection>>()
                    {
                        { null },
                        { null },
                        { null },
                        { null },
                        new List<GridDirection> { GridDirection.NorthWest, GridDirection.North, GridDirection.South, GridDirection.SouthEast },
                        { null },
                        { null },
                        { null },
                        { null },
                    }
                },
            };

            _restrictionPointsDictionary = new Dictionary<char, List<List<HashSet<int>>>>();
            foreach (var kvp in _restrictionsBaseDictionary)
            {
                _restrictionPointsDictionary[kvp.Key] = new List<List<HashSet<int>>>();
                for (int i = 0; i < 9; i++) _restrictionPointsDictionary[kvp.Key].Add(new List<HashSet<int>>());
                List<List<GridDirection>> countList = kvp.Value;
                for (int cnt = 0; cnt < countList.Count; cnt++)
                {
                    List<GridDirection> dirList = kvp.Value[cnt];
                    if (countList[cnt] != null)
                    {
                        _restrictionPointsDictionary[kvp.Key][cnt] = PointHelpers.GetIsoPoints(dirList);
                    }
                    else
                    {
                        _restrictionPointsDictionary[kvp.Key][cnt].Add(new HashSet<int>());
                    }
                }
            }
        }
    }
}
