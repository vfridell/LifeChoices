using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfLifeLib.Rules;

namespace GameOfLifeLib
{
    public static class LocationRuleFactory
    {
        private static List<ICARule> rules = new List<ICARule>();

        static LocationRuleFactory()
        {
        }
    }
}
