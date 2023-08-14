using GameOfLifeLib.Models.RuleChoosers;
using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLifeLib.Models.Games
{
    public class StripesGame : RandomGame
    {
        public StripesGame()
            : base(new Dictionary<string, int>()
            {
                { "B234" , 2 },
                { "B3/S45678" , 0 },
            }) { }
    }
}
