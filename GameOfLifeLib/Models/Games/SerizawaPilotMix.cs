using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    public class SerizawaPilotMix : ToroidGameBase
    {
        public override void Initialize()
        {
            base.Initialize();

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SerizawaGunship.rle");
            insertPoint = new Point(25, 25);
            InsertPattern(pattern, insertPoint, 0);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PilotSmall.rle");
            insertPoint = new Point(70, 70);
            InsertPattern(pattern, insertPoint, 1);
        }

    }
}
