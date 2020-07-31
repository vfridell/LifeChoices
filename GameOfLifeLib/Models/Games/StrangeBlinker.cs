using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    public class StrangeBlinker : ToroidGameBase
    {
        public override void Initialize()
        {
            base.Initialize();
            AllRules.Add(RuleFactory.GetRuleByName("Seeds"));
            _ruleChooser.SetRuleRank(RuleFactory.GetRuleByName("Seeds"), 2);

            CAPattern pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsJustFriendsStrangeBlinker.rle", false);
            Point insertPoint = new Point(25, 25);
            InsertPattern(pattern, insertPoint, 0);
            _rulePoints[new Point(26, 26)] = RuleFactory.GetRuleByName("Seeds");
            _rulePoints[new Point(27, 27)] = RuleFactory.GetRuleByName("Seeds");
        }

    }
}
