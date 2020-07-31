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
            base.Initialize();

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/coral.rle");
            insertPoint = new Point(5, 5);
            InsertPattern(pattern, insertPoint);
        }

    }
}
