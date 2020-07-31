using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    public class SeedsJustFriendsMix : ToroidGameBase
    {
        public override void Initialize(int gridSize, IRuleChooser ruleChooser)
        {
            base.Initialize(gridSize, ruleChooser);

            CAPattern pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/JustFriendsBox50.rle");
            Point insertPoint = new Point(25, 25);
            InsertPattern(pattern, insertPoint, 0);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsSmall.rle");
            insertPoint = new Point(2, 2);
            InsertPattern(pattern, insertPoint, 2);
        }

        public override void Initialize()
        {
            base.Initialize();

            CAPattern pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/JustFriendsBox50.rle");
            Point insertPoint = new Point(25, 25);
            InsertPattern(pattern, insertPoint, 0);

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/SeedsSmall.rle");
            insertPoint = new Point(2, 2);
            InsertPattern(pattern, insertPoint, 2);
        }

    }
}

