using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLifeLib.Parsers;
using GameOfLifeLib.Rules;

namespace GameOfLifeLib.Models.RuleChoosers
{
    public class ElementalRuleChooser : RankedRuleChooser
    {
        public Dictionary<ICARule, ElementalCombo> RuleElements = new Dictionary<ICARule, ElementalCombo>();

        public override string Name => "ElementalRuleChooser";

        public override ICARule Choose(Random random, IHazGame game, List<Point> nPoints)
        {
            Dictionary<ICARule, int> localRules = nPoints.Select(p => game.RulePoints.TryGetValue(p, out ICARule r) ? r : RuleFactory.DefaultRule)
                                             .GroupBy(r => r)
                                             .ToDictionary(g => g.Key, g => g.Count());
            localRules.Remove(RuleFactory.DefaultRule);
            if (!localRules.Any())
            {
                return game.AllRules[0];
            }
            else if (localRules.Count == 1)
            {
                return localRules.First().Key;
            }

            // Neutral
            int ruleInfluenceMinRank = localRules.Min(k => _ruleInfluenceValues[RulesRankDictionary[k.Key], k.Value - 1]);
            List<ICARule> choices = localRules.Where(k => _ruleInfluenceValues[RulesRankDictionary[k.Key], k.Value - 1] == ruleInfluenceMinRank)
                                              .Select(r => r.Key)
                                              .ToList();
            if (choices.Count == 1)
            {
                return choices[0];
            }
            else
            {

                ICARule air = RuleElements.FirstOrDefault(kvp => kvp.Value.Air && kvp.Value.ElementCount == 1).Key;
                ICARule earth = RuleElements.FirstOrDefault(kvp => kvp.Value.Earth && kvp.Value.ElementCount == 1).Key;
                ICARule fire = RuleElements.FirstOrDefault(kvp => kvp.Value.Fire && kvp.Value.ElementCount == 1).Key;
                ICARule water = RuleElements.FirstOrDefault(kvp => kvp.Value.Water && kvp.Value.ElementCount == 1).Key;
                ElementalCombo ec = new ElementalCombo(localRules.SelectMany(kvp => RuleElements[kvp.Key].Elements).ToArray());
                switch (ec.ElementCount)
                {
                    case 1:
                        throw new Exception("Not possible");
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
                        // pick something
                        break;
                    default:
                        throw new Exception("too many elements.");
                }
            }

            return choices[random.Next(0, choices.Count - 1)]; 
        }
    }

    public struct ElementalCombo
    {
        public ElementalCombo(Element element)
        {
            ElementCount = 1;
            Air = element == Element.Air;
            Earth = element == Element.Earth;
            Fire = element == Element.Fire;
            Water = element == Element.Water;
        }

        public ElementalCombo(params Element[] elements)
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
}
