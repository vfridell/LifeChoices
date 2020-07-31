using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLifeLib.Parsers;
using GameOfLifeLib.Rules;

namespace GameOfLifeLib.Models.RuleChoosers
{
    // Rank is decided by more influence preferring rules with fewer states. Rules with fewer states need less influence to "win" the cell
    // Ties are broken by random choice, but ties are less likely than pure majority rule
    public class FewerStatesRuleChooser : RankedRuleChooser
    {
        public override string Name => nameof(FewerStatesRuleChooser);

        public override ICARule Choose(Random random, IHazGame game, List<Point> nPoints)
        {
            ICARule rule;
            Dictionary<ICARule, int> localRules = nPoints.Select(p => game.RulePoints.TryGetValue(p, out ICARule r) ? r : RuleFactory.DefaultRule)
                                             .GroupBy(r => r)
                                             .ToDictionary(g => g.Key, g => g.Count());
            localRules.Remove(RuleFactory.DefaultRule);
            if (!localRules.Any())
            {
                return game.AllRules[random.Next(0, game.AllRules.Count - 1)];
            }
            else if (localRules.Count == 1)
            {
                return localRules.First().Key;
            }
            int ruleInfluenceMinRank = localRules.Min(k => _ruleInfluenceValues[k.Key.NumStates - 2, k.Value - 1]);
            List<ICARule> choices = localRules.Where(k => _ruleInfluenceValues[k.Key.NumStates - 2, k.Value - 1] == ruleInfluenceMinRank)
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
        }
    }
}
