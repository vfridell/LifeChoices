using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    public class LifeGliderGunMix : ToroidGameBase
    {
        public override void Initialize()
        {
            base.Initialize();

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/JustFriendsBox50.rle");
            insertPoint = new Point(25, 25);
            InsertPattern(pattern, insertPoint, 1);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/GliderGunNE.rle");
            insertPoint = new Point(48, 31);
            InsertPattern(pattern, insertPoint, 0);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PilotSmall.rle");
            insertPoint = new Point(1, 1);
            InsertPattern(pattern, insertPoint, 2);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/PilotSmall.rle");
            insertPoint = new Point(5, 5);
            InsertPattern(pattern, insertPoint, 2);
        }

    }
}
