using Antlr4.Runtime;
using GameOfLifeLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Collections.Concurrent;

namespace GameOfLifeLib.Parsers
{
    public class RuleFactory
    {
        public static ICARule DefaultRule { get; private set; } = new NullRule();

        public static ICARule GetRuleFromFile(string name, string filename)
        {
            AntlrFileStream fileStream = new AntlrFileStream(filename, Encoding.UTF8);
            RuleTableLexer ruleTableLexer = new RuleTableLexer(fileStream);
            Antlr4.Runtime.CommonTokenStream tokenStream = new CommonTokenStream(ruleTableLexer);
            RuleTableParser ruleTableParser = new RuleTableParser(tokenStream);
            ruleTableParser.RemoveErrorListeners();
            ruleTableParser.AddErrorListener(new ThrowingErrorListener());
            RuleTableParser.TableDefContext context = ruleTableParser.tableDef();
            RuleTableListener listener = new RuleTableListener();
            Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(listener, context);

            if (listener.Symmetry == RuleSymmetry.permute)
                return new RuleTableCountStatesRule(name, listener.Neighborhood, listener.Symmetry, listener.NumStates, listener.TransitionDictionary);
            else
                return new RuleTableRule(name, listener.Neighborhood, listener.Symmetry, listener.NumStates, listener.TransitionDictionary);
        }

        static ConcurrentDictionary<string, ICARule> _rulesCache { get; set; } = new ConcurrentDictionary<string, ICARule>();
        public static ICARule GetRuleByName(string name)
        {
            if (_rulesCache.TryGetValue(name, out ICARule rule))
                return rule;
            else
            {
                if(LifeLikeRule.LifeLikeRuleString(name))
                {
                    rule = new LifeLikeRule(name, name);
                }
                else if (name.Trim().ToLower().Equals("life"))
                    rule = new LifeRule();
                else if (name.Trim().ToLower().Equals("highlife"))
                    rule = new HighLifeRule();
                else if (name.Trim().ToLower().Equals("seeds"))
                    rule = new SeedsRule();
                else if (name.Trim().ToLower().Equals("null"))
                    rule = new NullRule();
                else
                    rule = GetRuleFromFile(name, $"RuleFiles/{name}.table");
                _rulesCache[name] = rule;
                return rule;
            }
        }

    }

    public class RuleTableListener : RuleTableBaseListener
    {
        public int NumStates { get; set; }
        public CANeighborhood Neighborhood { get; set; }
        public RuleSymmetry Symmetry { get; set; }
        public Dictionary<string, List<int>> Variables { get; set; } = new Dictionary<string, List<int>>();
        public List<List<string>> TransitionStringLists { get; set; } = new List<List<string>>();
        public List<List<string>> ExpandedTransitionStringLists { get; set; } = new List<List<string>>();
        public List<List<int>> TransitionLists { get; set; } = new List<List<int>>();
        public Dictionary<string, int> TransitionDictionary { get; set; } = new Dictionary<string, int>();

        private int transitionListItemCount;
        private int transitionListsIndex;
        private string currentVarName;

        public override void EnterStates([NotNull] RuleTableParser.StatesContext context)
        {
            NumStates = int.Parse(context.children[2].GetText());
        }

        public override void EnterNeighborhood([NotNull] RuleTableParser.NeighborhoodContext context)
        {
            Neighborhood = (CANeighborhood)Enum.Parse(typeof(CANeighborhood), context.children[2].GetText());
            switch (Neighborhood)
            {
                case CANeighborhood.Moore:
                    transitionListItemCount = 10;
                    break;
                case CANeighborhood.vonNeumann:
                    transitionListItemCount = 6;
                    break;
                default:
                    throw new Exception($"Neighborhood not supported: {Neighborhood}");
            }
        }

        public override void EnterSym([NotNull] RuleTableParser.SymContext context)
        {
            Symmetry = (RuleSymmetry)Enum.Parse(typeof(RuleSymmetry), context.children[2].GetText());
        }

        public override void EnterVarline([NotNull] RuleTableParser.VarlineContext context)
        {
            currentVarName = context.children[1].GetText();
            if (!Variables.ContainsKey(currentVarName)) Variables.Add(currentVarName, new List<int>());
        }

        public override void EnterListitem([NotNull] RuleTableParser.ListitemContext context)
        {
            Variables[currentVarName].Add(int.Parse(context.GetText()));
        }

        public override void EnterTransitionline([NotNull] RuleTableParser.TransitionlineContext context)
        {
            TransitionStringLists.Add(new List<string>());
        }

        public override void EnterTransitionitem([NotNull] RuleTableParser.TransitionitemContext context)
        {
            TransitionStringLists[transitionListsIndex].Add(context.GetText());
        }

        public override void ExitTransitionline([NotNull] RuleTableParser.TransitionlineContext context)
        {
            if (TransitionStringLists[transitionListsIndex].Count != transitionListItemCount)
                throw new Exception($"Transition lists must contain exactly {transitionListItemCount} items for neighborhood={Neighborhood} ");

            ExpandTransitionListVars(transitionListsIndex);
            transitionListsIndex++;
        }

        public override void ExitTableDef([NotNull] RuleTableParser.TableDefContext context)
        {
            if (Symmetry == RuleSymmetry.permute)
            {
                CreateCountingDictionary();
            }
            else
            {
                ExpandSymmetry();
                CreateTransitionDictionary();
            }
        }

        private void CreateCountingDictionary()
        {
            int neighborhoodSize = Neighborhood == CANeighborhood.Moore ? 9 : 5;
            foreach (List<string> transitionStringList in ExpandedTransitionStringLists)
            {
                List<string> sortedNeighborhood = transitionStringList.Skip(1).Take(neighborhoodSize - 1).OrderBy(s => int.Parse(s)).ToList();
                sortedNeighborhood.Insert(0, transitionStringList[0]);
                string keyString = GetKeyString(sortedNeighborhood);
                if (!TransitionDictionary.ContainsKey(keyString)) TransitionDictionary.Add(keyString, int.Parse(transitionStringList.Last()));
            }
        }

        private void CreateTransitionDictionary()
        {
            TransitionDictionary.Clear();
            int neighborhoodSize = Neighborhood == CANeighborhood.Moore ? 9 : 5;
            foreach(List<string> transitionStringList in ExpandedTransitionStringLists)
            {
                string keyString = GetKeyString(transitionStringList.Take(neighborhoodSize));
                if (!TransitionDictionary.ContainsKey(keyString)) TransitionDictionary.Add(keyString, int.Parse(transitionStringList.Last()));
            }
        }

        private string GetKeyString<T>(IEnumerable<T> neighborhood) => neighborhood.Skip(1).Aggregate(neighborhood.First().ToString(), (s2, s) => s2 + "," + s.ToString());

        private void ExpandSymmetry()
        {
            if (Symmetry == RuleSymmetry.none || Symmetry == RuleSymmetry.permute) return;

            List<List<string>> NewTransitionStringLists = new List<List<string>>();
            List<List<int>> transforms = GetSymmetryTransforms();

            foreach(List<string> transition in ExpandedTransitionStringLists)
            {
                NewTransitionStringLists.Add(transition);
                foreach(List<int> transform in transforms)
                {
                    List<string> congruentTransition = new List<string>();
                    congruentTransition.Add(transition[0]);
                    for (int i = 0; i < transform.Count; i++) congruentTransition.Add(transition[transform[i]]);
                    congruentTransition.Add(transition.Last());
                    NewTransitionStringLists.Add(congruentTransition);
                }
            }

            ExpandedTransitionStringLists = NewTransitionStringLists;
        }

        private List<List<int>> GetSymmetryTransforms()
        {
            List<List<int>> allTransforms;
            List<int> addList = null;
            if (Neighborhood == CANeighborhood.Moore)
            {
                allTransforms = new List<List<int>>()
                {
                    new List<int> {5,4,3,2,1,8,7,6 }, //0 reflect horizontal
                    new List<int> {1,8,7,6,5,4,3,2 }, //1 reflect vert
                    new List<int> {8,1,2,3,4,5,6,7 }, //2 rotate clockwise 1 cell
                    new List<int> {7,8,1,2,3,4,5,6 }, //3 rotate clockwise 2 cells
                    new List<int> {6,7,8,1,2,3,4,5 }, //4 rotate clockwise 3 cells
                    new List<int> {5,6,7,8,1,2,3,4 }, //5 rotate clockwise 4 cells
                    new List<int> {4,5,6,7,8,1,2,3 }, //6 rotate clockwise 5 cells
                    new List<int> {3,4,5,6,7,8,1,2 }, //7 rotate clockwise 6 cells
                    new List<int> {2,3,4,5,6,7,8,1 }, //8 rotate clockwise 7 cells
                    new List<int> {1,2,3,4,5,6,7,8 }, //9 rotate clockwise 8 cells
                };

                switch (Symmetry)
                {
                    case RuleSymmetry.none:
                        return null;
                    case RuleSymmetry.permute:
                        // ignore order. just count
                        break;
                    case RuleSymmetry.reflect_horizontal:
                        addList = new List<int>() { 0, 1 };
                        break;
                    case RuleSymmetry.rotate4:
                        addList = new List<int>() { 3,5,7 };
                        break;
                    case RuleSymmetry.rotate4reflect:
                        addList = new List<int>() { 0,1,3,5,7 };
                        break;
                    case RuleSymmetry.rotate8:
                        addList = new List<int>() { 2,3,4,5,6,7,8 };
                        break;
                    case RuleSymmetry.rotate8reflect:
                        addList = new List<int>() { 0,1,2,3,4,5,6,7,8 };
                        break;
                    default:
                        throw new Exception($"Unsupported RuleSymmetry: {Symmetry}");
                }

            }
            else if (Neighborhood == CANeighborhood.vonNeumann)
            {
                allTransforms = new List<List<int>>()
                {
                    new List<int> {3,2,1,4 }, //0 reflect horizontal
                    new List<int> {1,4,3,2 }, //1 reflect vert
                    new List<int> {4,1,2,3 }, //2 rotate clockwise 1 cell
                    new List<int> {3,4,1,2 }, //3 rotate clockwise 2 cells
                    new List<int> {2,3,4,1 }, //4 rotate clockwise 3 cells
                    new List<int> {1,2,3,4 }, //5 rotate clockwise 4 cells
                };

                switch (Symmetry)
                {
                    case RuleSymmetry.none:
                        return null;
                    case RuleSymmetry.permute:
                        // ignore order. just count
                        break;
                    case RuleSymmetry.reflect_horizontal:
                        addList = new List<int>() { 0,1 };
                        break;
                    case RuleSymmetry.rotate4:
                        addList = new List<int>() { 2,3,4 };
                        break;
                    case RuleSymmetry.rotate4reflect:
                        addList = new List<int>() { 0,1,2,3,4 };
                        break;
                    case RuleSymmetry.rotate8:
                        throw new Exception($"Unsupported vonNeumann RuleSymmetry: {Symmetry}");
                    case RuleSymmetry.rotate8reflect:
                        throw new Exception($"Unsupported vonNeumann RuleSymmetry: {Symmetry}");
                    default:
                        throw new Exception($"Unsupported RuleSymmetry: {Symmetry}");
                }
            }
            else
            {
                throw new Exception($"Unsupported neighborhood {Neighborhood}");
            }

            List<List<int>> result = new List<List<int>>();
            foreach(int addIndex in addList)
            {
                result.Add(allTransforms[addIndex]);
            }
            return result;
        }

        private void ExpandTransitionListVars(int transitionListsIndex)
        {
            List<string> currentList = TransitionStringLists[transitionListsIndex];
            HashSet<string> vars = currentList.Where(s => s.Any(c => char.IsLetter(c))).ToHashSet();
            ExpandedTransitionStringLists.Add(currentList.ToList());
            foreach(string v in vars)
            {
                if (!Variables.ContainsKey(v)) throw new Exception($"Variable {v} not defined before it is used");
                foreach(int val in Variables[v])
                {
                    string valString = val.ToString();
                    List<List<string>> newLists = new List<List<string>>();
                    foreach(List<string> l in ExpandedTransitionStringLists.Where(l => l.Contains(v)))
                    {
                        List<string> newTransitionList = l.Select(s => s.Equals(v) ? valString : s).ToList();
                        newLists.Add(newTransitionList);
                    }
                    ExpandedTransitionStringLists.AddRange(newLists);
                }

            }
            ExpandedTransitionStringLists.RemoveAll(l => l.Any(s => s.Any(c => char.IsLetter(c))));

        }
    }

    public class ThrowingErrorListener : BaseErrorListener
    {
        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
             throw new Exception("line " + line + ":" + charPositionInLine + " " + msg);
        }
    }
}
