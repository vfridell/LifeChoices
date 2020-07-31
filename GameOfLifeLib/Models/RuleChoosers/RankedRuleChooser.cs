using GameOfLifeLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLifeLib.Models.RuleChoosers
{
    public abstract class RankedRuleChooser : IRuleChooser
    {
        public abstract string Name { get; }
        public abstract ICARule Choose(Random random, IHazGame game, List<Point> nPoints);

        public Dictionary<ICARule, int> RulesRankDictionary { get; protected set; } = new Dictionary<ICARule, int>();
        public void SetRuleRank(ICARule rule, int rank)
        {
            RulesRankDictionary[rule] = rank;
        }

        public virtual void SetRuleRank(List<ICARule> rules)
        {
            int index = 0;
            RulesRankDictionary = rules.ToDictionary(r => r, r => index++);
        }

        public bool TryGetRuleRank(ICARule rule, out int rank) => RulesRankDictionary.TryGetValue(rule, out rank);

        // ruleInfluenceValues[i,j] 
        // i = ranked criteria (e.g. index of rule in AllRulesRank list) OR (number of states in Rule - 2)
        // j = influence on center cell (i.e. number of cells with this rule in the neighborhood - 1)
        protected int[,] _ruleInfluenceValues = new int[7, 7]
        {
            {128, 64, 32, 16, 8, 4, 2 },
            {256, 128, 64, 32, 16, 8, 4 },
            {512, 256, 128, 64, 32, 16, 8 },
            {1024, 512, 256, 128, 64, 32, 16 },
            {2048, 1024, 512, 256, 128, 64, 32 },
            {4092, 2048, 1024, 512, 256, 128, 64 },
            {8184, 4092, 2048, 1024, 512, 256, 128 },
        };
    }
}
