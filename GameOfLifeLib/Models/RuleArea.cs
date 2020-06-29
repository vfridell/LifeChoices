using GameOfLifeLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Models
{
    public class RuleArea : IHaveRectangle
    {
        public Rect Rectangle { get; set; }
        public ICARule Rule { get; set; }
        public int ZOrder { get; set; }
    }
}
