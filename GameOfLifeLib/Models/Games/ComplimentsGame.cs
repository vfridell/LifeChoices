using GameOfLifeLib.Models.RuleChoosers;
using GameOfLifeLib.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLifeLib.Models.Games
{
    public class ComplimentsGame : RandomGame
    {
        // vary the ranks and this stays chaotic with stable sections despite it
        // shows good survivability of both rules
        public ComplimentsGame()
            : base(new Dictionary<string, int>()
            {
                { "B35/S2345" , 6 },
                {"B56/S12" , 0}
            }) { }
    }
}
