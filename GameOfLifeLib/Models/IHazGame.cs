using GameOfLifeLib.Rules;
using System.Collections.Generic;

namespace GameOfLifeLib.Models
{
    public interface IHazGame
    {
        void Initialize();
        void ExecuteGameLoop();
        IRuleChooser RuleChooser { get; }
        PieceGrid CurrentGeneration { get; }
        Dictionary<Point, ICARule> RulePoints { get; }
        List<ICARule> AllRules { get; }
    }
}
