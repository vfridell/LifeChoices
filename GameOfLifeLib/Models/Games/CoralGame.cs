using GameOfLifeLib.Models.RuleChoosers;
using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    public class CoralGame : ToroidGameBase
    {
        public override void Initialize()
        {
            base.Initialize(60, new MajorityInOrderRuleChooser());

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/coral.rle");
            insertPoint = new Point(5, 5);
            InsertPattern(pattern, insertPoint, 5);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/persianrugs.rle");
            insertPoint = new Point(55, 55);
            InsertPattern(pattern, insertPoint, 1);
        }

    }
}
