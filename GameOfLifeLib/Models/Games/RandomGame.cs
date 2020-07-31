using GameOfLifeLib.Parsers;
using GameOfLifeLib.Rules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    public class RandomGame : ToroidGameBase
    {
        private Dictionary<string, int> _ruleNameRankDictionary;
        public ReadOnlyDictionary<string, int> RuleNameRankDictionary => new ReadOnlyDictionary<string, int>(_ruleNameRankDictionary);

        public RandomGame(Dictionary<string, int> ruleNameRankDictionary)
        {
            _ruleNameRankDictionary = ruleNameRankDictionary;
        }

        public override void Initialize(int gridSize, IRuleChooser ruleChooser)
        {
            base.Initialize(gridSize, ruleChooser);
            SetupRulesRank();
            SetupRandomPieceGrid();
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupRulesRank();
            SetupRandomPieceGrid();
        }

        private void SetupRandomPieceGrid()
        {
            foreach (var kvp in _currentGen.PointPieces.ToArray())
            {
                ICARule rule = AllRules[_random.Next(0, AllRules.Count)];
                int stateValue = _random.Next(0, rule.NumStates);
                _currentGen.PointPieces[kvp.Key] = Piece.Get(stateValue);
                _rulePoints[kvp.Key] = rule;
            }
        }

        private void SetupRulesRank()
        {
            foreach (var kvp in _ruleNameRankDictionary)
            {
                _allRules.Add(RuleFactory.GetRuleByName(kvp.Key));
                _ruleChooser.SetRuleRank(RuleFactory.GetRuleByName(kvp.Key), kvp.Value);
            }
        }
    }
}
