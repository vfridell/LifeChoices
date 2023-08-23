using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    public class SalvoGliderGuns : ToroidGameBase
    {
        public override void Initialize()
        {
            base.Initialize();

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SalvoGliderGun.rle");
            insertPoint = new Point(25, 25);
            InsertPattern(pattern, insertPoint, 0);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SalvoGliderGun2.rle");
            insertPoint = new Point(15, 15);
            InsertPattern(pattern, insertPoint, 0);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SalvoGliderGun3.rle");
            insertPoint = new Point(45, 45);
            InsertPattern(pattern, insertPoint, 0);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SalvoGliderGun5.rle");
            insertPoint = new Point(65, 65);
            InsertPattern(pattern, insertPoint, 0);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SalvoGliderGun6.rle");
            insertPoint = new Point(75, 75);
            InsertPattern(pattern, insertPoint, 0);
        }

    }
}
