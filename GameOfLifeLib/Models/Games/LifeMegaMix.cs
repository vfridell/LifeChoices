using GameOfLifeLib.Models.RuleChoosers;
using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    public class LifeMegaMix : ToroidGameBase
    {
        public override void Initialize()
        {
            base.Initialize();
            _ruleChooser = new MajorityInOrderRuleChooser();

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PersianRugs.rle");
            insertPoint = new Point(5, 5);
            InsertPattern(pattern, insertPoint, 5);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SpiralGrowth.rle");
            insertPoint = new Point(85, 45);
            InsertPattern(pattern, insertPoint, 4);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/JustFriendsBox50.rle");
            insertPoint = new Point(25, 25);
            InsertPattern(pattern, insertPoint, 0);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            insertPoint = new Point(48, 31);
            InsertPattern(pattern, insertPoint, 1);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsSmall.rle");
            insertPoint = new Point(2, 2);
            InsertPattern(pattern, insertPoint, 3);
        }

    }
}
