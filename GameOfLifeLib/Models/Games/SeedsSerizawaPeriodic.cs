using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models.Games
{
    public class SeedsSerizawaPeriodic : ToroidGameBase
    {
        public override void Initialize()
        {
            base.Initialize();
            AllRules.Add(RuleFactory.GetRuleByName("Seeds"));
            AllRules.Add(RuleFactory.GetRuleByName("Serizawa"));
            _ruleChooser.SetRuleRank(RuleFactory.GetRuleByName("Seeds"), 4);
            _ruleChooser.SetRuleRank(RuleFactory.GetRuleByName("Serizawa"), 0);

            PieceGrid pieceGrid = new PieceGrid(8);
            var layout = new Dictionary<Point, Piece> {
                { new Point(3,4), Piece.Get(2) },
                { new Point(4,3), Piece.Get(2) },
            };
            pieceGrid.Initialize(layout);
            CAPattern pattern = new CAPattern(pieceGrid, RuleFactory.GetRuleByName("Serizawa"));
            InsertPattern(pattern, new Point(25, 25));
            InsertPattern(pattern, new Point(35, 25));

            _rulePoints[new Point(28, 28)] = RuleFactory.GetRuleByName("Seeds");
        }

    }
}
