using GameOfLifeLib.Models.RuleChoosers;
using GameOfLifeLib.Parsers;
using GameOfLifeLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    // designed to be used with ElementalRuleChooser
    public class ElementalGame : ToroidGameBase
    {
        public override void Initialize()
        {
            Initialize(100, new ElementalRuleChooser());
        }

        public override void Initialize(int gridSize, IRuleChooser ruleChooser)
        {
            if (!(ruleChooser is ElementalRuleChooser)) throw new ArgumentException("You must use the ElementalRuleChooser with this game");
            base.Initialize(gridSize, ruleChooser);

            //List<string> ruleNames = new List<string> { "B3/S45678", "JustFriends", "Life", "Seeds" };
            //List<string> ruleNames = new List<string> { "B3/S45678", "Life", "JustFriends", "Seeds" };
            //List<string> ruleNames = new List<string> { "B3/S45678", "Seeds", "Life", "JustFriends" };
            //List<string> ruleNames = new List<string> { "B3/S45678", "JustFriends", "Seeds", "Life" };
            List<string> ruleNames = new List<string> { "JustFriends", "B3/S45678", "Life", "Seeds" };
            foreach (string ruleName in ruleNames) AllRules.Add(RuleFactory.GetRuleByName(ruleName));

            ICARule earth = AllRules[0];
            ICARule air = AllRules[1];
            ICARule water = AllRules[2];
            ICARule fire = AllRules[3];
            ICARule airFire = RuleFactory.GetStrengthenRule(air, fire);
            ICARule earthWater = RuleFactory.GetStrengthenRule(earth, water);
            ICARule airEarth = RuleFactory.GetWeakenRule(air, earth);
            ICARule fireWater = RuleFactory.GetWeakenRule(fire, water);

            AllRules.Add(airFire);
            AllRules.Add(earthWater);
            AllRules.Add(airEarth);
            AllRules.Add(fireWater);

            ((ElementalRuleChooser)_ruleChooser).RuleElements.Add(earth, new ElementalCombo(Element.Earth));
            ((ElementalRuleChooser)_ruleChooser).RuleElements.Add(air, new ElementalCombo(Element.Air));
            ((ElementalRuleChooser)_ruleChooser).RuleElements.Add(water, new ElementalCombo(Element.Water));
            ((ElementalRuleChooser)_ruleChooser).RuleElements.Add(fire, new ElementalCombo(Element.Fire));
            ((ElementalRuleChooser)_ruleChooser).RuleElements.Add(airFire, new ElementalCombo(Element.Air, Element.Fire));
            ((ElementalRuleChooser)_ruleChooser).RuleElements.Add(earthWater, new ElementalCombo(Element.Earth, Element.Water));
            ((ElementalRuleChooser)_ruleChooser).RuleElements.Add(airEarth, new ElementalCombo(Element.Air, Element.Earth));
            ((ElementalRuleChooser)_ruleChooser).RuleElements.Add(fireWater, new ElementalCombo(Element.Fire, Element.Water));

            _ruleChooser.SetRuleRank(RuleFactory.GetRuleByName("B3/S45678"), 3);
            _ruleChooser.SetRuleRank(RuleFactory.GetRuleByName("JustFriends"), 0);
            _ruleChooser.SetRuleRank(RuleFactory.GetRuleByName("Life"), 0);
            _ruleChooser.SetRuleRank(RuleFactory.GetRuleByName("Seeds"), 2);
            _ruleChooser.SetRuleRank(airFire, 6);
            _ruleChooser.SetRuleRank(earthWater, 6);
            _ruleChooser.SetRuleRank(airEarth, 6);
            _ruleChooser.SetRuleRank(fireWater, 6);

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/JustFriendsBox50.rle");
            insertPoint = new Point(25, 25);
            InsertPattern(pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/coral.rle");
            insertPoint = new Point(5, 5);
            InsertPattern(pattern, insertPoint);

            //pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            //insertPoint = new Point(48, 31);
            //InsertPattern(rulePoints, currentGen, pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            insertPoint = new Point(95, 95);
            InsertPattern(pattern, insertPoint);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsSmall.rle");
            insertPoint = new Point(90, 90);
            InsertPattern(pattern, insertPoint);
        }


    }
}
