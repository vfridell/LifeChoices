using GameOfLifeLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models
{
    public interface IRuleChooser
    {
        string Name { get; }
        ICARule Choose(Random random, IHazGame game, List<Point> nPoints);
        void SetRuleRank(ICARule rule, int rank);
        bool TryGetRuleRank(ICARule rule, out int rank);
    }
}
