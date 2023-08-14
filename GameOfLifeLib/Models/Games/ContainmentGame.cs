using GameOfLifeLib.Models.RuleChoosers;
using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLifeLib.Models.Games
{
    public class ContainmentGame : RandomGame
    {
        public ContainmentGame()
            : base(new Dictionary<string, int>()
            {
                { "B256/S12" , 2 },
                {"B56/S14568" , 0}
            }) {}
    }
}
