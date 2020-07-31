using GameOfLifeLib.Models.RuleChoosers;
using GameOfLifeLib.Parsers;
using GameOfLifeLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLifeLib.Models
{
    public class ToroidGameBase : IHazGame
    {
        protected bool _initialized = false;
        protected virtual PieceGrid _currentGen { get; set; }
        protected virtual Dictionary<Point, ICARule> _rulePoints { get; set; }
        protected IRuleChooser _ruleChooser;
        protected Random _random;
        protected List<ICARule> _allRules;

        protected Dictionary<Point, int> _rulePointsAge { get; set; }

        public virtual void Initialize()
        {
            _random = new Random();
            _allRules = new List<ICARule>();
            _rulePoints = new Dictionary<Point, ICARule>();
            _rulePointsAge = new Dictionary<Point, int>();
            _currentGen = new PieceGrid(100);
            _currentGen.Initialize();
            _ruleChooser = new MajorityInOrderRuleChooser();
        }

        public virtual void Initialize(int gridSize, IRuleChooser ruleChooser)
        {
            _random = new Random();
            _allRules = new List<ICARule>();
            _rulePoints = new Dictionary<Point, ICARule>();
            _rulePointsAge = new Dictionary<Point, int>();
            _currentGen = new PieceGrid(gridSize);
            _currentGen.Initialize();
            _ruleChooser = ruleChooser;
        }

        public virtual PieceGrid CurrentGeneration => _currentGen;
        public virtual Dictionary<Point, ICARule> RulePoints => _rulePoints;
        public List<ICARule> AllRules => _allRules;

        public IRuleChooser RuleChooser => _ruleChooser;

        public virtual void ExecuteGameLoop()
        {
            PieceGrid nextGen = _currentGen.Clone();
            Dictionary<Point, ICARule> nextRulePoints = new Dictionary<Point, ICARule>();

            foreach (var kvp in _currentGen.PointPieces)
            {
                bool existingRule = _rulePoints.TryGetValue(kvp.Key, out ICARule rule);
                List<Point> nPoints = PointHelpers.GetAdjacentPointsToroid(kvp.Key, _currentGen, PointHelpers.NeighborhoodOrder.Moore).ToList();
                bool aliveNeighbors = nPoints.Any(p => _currentGen.PointPieces[p].StateValue > 0);
                if (!existingRule && !aliveNeighbors) continue;

                if (!existingRule)
                {
                    rule = _ruleChooser.Choose(_random, this, nPoints);
                    nextRulePoints.Add(kvp.Key, rule);
                    _rulePointsAge[kvp.Key] = 1;
                }
                else if (!aliveNeighbors)
                {
                    _rulePointsAge[kvp.Key] = 0;
                }
                else
                {
                    nextRulePoints.Add(kvp.Key, rule);
                    if (_rulePointsAge.ContainsKey(kvp.Key)) _rulePointsAge[kvp.Key]++;
                    else _rulePointsAge[kvp.Key] = 1;
                }
                nextGen.PointPieces[kvp.Key] = rule.Run(_currentGen, kvp.Key);
            }

            _rulePoints = nextRulePoints;
            _currentGen = nextGen;
        }

        public void InsertPattern(CAPattern pattern, Point insertPoint, int rank = -1)
        {
            if (!_allRules.Contains(pattern.Rule))
            {
                _allRules.Add(pattern.Rule);
            }
            if (!_ruleChooser.TryGetRuleRank(pattern.Rule, out int existingRank))
            {
                if (rank >= 0)
                    _ruleChooser.SetRuleRank(pattern.Rule, rank);
                else
                    _ruleChooser.SetRuleRank(pattern.Rule, _allRules.IndexOf(pattern.Rule));
            }

            if (_currentGen.IsOutOfBounds(insertPoint)) throw new ArgumentException($"Insert point {insertPoint} is outside given PieceGrid (size {_currentGen.Size})");
            if (pattern.Pattern.Size > _currentGen.Size) throw new ArgumentException($"Pattern too big for given PieceGrid: {_currentGen.Size} < {pattern.Pattern.Size}");
            foreach (var kvp in pattern.Pattern.PointPieces)
            {
                IEnumerable<Point> nPoints = kvp.Key.GetAdjacentPointsNotOutOfBounds(pattern.Pattern, PointHelpers.NeighborhoodOrder.Moore);
                bool aliveNeighbors = nPoints.Any(pt => pattern.Pattern.PointPieces[pt].StateValue > 0);
                if (aliveNeighbors)
                {
                    Point p = insertPoint.AddPointsToroid(kvp.Key, _currentGen);
                    _currentGen.PointPieces[p] = kvp.Value;
                    _rulePoints[p] = pattern.Rule;
                }
            }
        }
    }
}
