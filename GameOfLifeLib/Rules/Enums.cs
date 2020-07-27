using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeLib.Rules
{
    public enum CANeighborhood { vonNeumann, Moore }
    public enum RuleSymmetry { none, rotate4, rotate8, rotate4reflect, rotate8reflect, reflect_horizontal, permute}
    public enum Owner { None = 0, Player1 = 1, Player2 = 2 }
    public enum PieceAspect { Natural = 0, Played = 1 }
    public enum Element { Fire, Earth, Water, Air }
}
