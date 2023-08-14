using GameOfLifeLib.Models.RuleChoosers;
using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLifeLib.Models.Games
{
    public class SeedsNonIsoGame : RandomGame
    {
        public SeedsNonIsoGame()
            : base(new Dictionary<string, int>()
            {
                { "Seeds" , 3 },
                { "B2ci3ai4c8/S02ae3eijkq4iz5ar6i7e" , 0 },
            }) { }
    }
}
