using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    public class ShipShipGame : ToroidGameBase
    {
        public override void Initialize()
        {
            base.Initialize();

            CAPattern pattern;
            Point insertPoint;

            pattern = PatternFactory.GetPieceGridFromPatternFile("RuleFiles/ShipShip.rle");
            insertPoint = new Point(25, 25);
            InsertPattern(pattern, insertPoint, 0);
        }

    }
}
