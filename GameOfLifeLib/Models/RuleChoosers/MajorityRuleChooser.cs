using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLifeLib.Parsers;
using GameOfLifeLib.Rules;

namespace GameOfLifeLib.Models.RuleChoosers
{
    // Rank is decided by more influence. ties are broken by random choice
    public class MajorityRuleChooser : RankedRuleChooser
    {
        public override string Name => nameof(MajorityRuleChooser);

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

            List<ICARule> choices = localRules.Where(c => c.Value == localRules.Max(k => k.Value)).Select(r => r.Key).ToList();
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
