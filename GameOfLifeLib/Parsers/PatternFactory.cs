using GameOfLifeLib.Rules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameOfLifeLib.Parsers
{
    public class PatternFactory
    {
        static Regex headerRegex = new Regex(@"^x\s*=\s*([0-9]+)\s*,\s*y\s*=\s*([0-9]+)\s*,\s*rule\s*=\s*(.*)");

        public static CAPattern GetPieceGridFromPatternFile(string filename)
        {
            ICARule rule;
            PieceGrid newGrid;
            using (StreamReader reader = new StreamReader(File.OpenRead(filename)))
            {
                string line = reader.ReadLine();

                while (!line.Trim().Any() || line.StartsWith("#"))
                {
                    line = reader.ReadLine();
                }
                if (!headerRegex.IsMatch(line)) throw new Exception("Bad RLE file. Header not recognized");

                MatchCollection headerMatchCollection = headerRegex.Matches(line);
                int patternWidth = int.Parse(headerMatchCollection[0].Groups[1].Value);
                int patternHeight = int.Parse(headerMatchCollection[0].Groups[2].Value);
                string ruleName = headerMatchCollection[0].Groups[3].Value;

                rule = RuleFactory.GetRuleByName(ruleName);

                char c = (char)reader.Read();
                StringBuilder numAccumulator = new StringBuilder();
                int num = 1;
                int x = 0;
                int y = 0;
                Point currentPoint = new Point(x, y);
                newGrid = new PieceGrid(Math.Max(patternWidth, patternHeight));
                newGrid.Initialize();
                while (!reader.EndOfStream)
                {
                    if (char.IsDigit(c))
                    {
                        numAccumulator.Append(c);
                    }
                    else
                    {
                        if (numAccumulator.Length > 0)
                        {
                            num = int.Parse(numAccumulator.ToString());
                            numAccumulator.Clear();
                        }
                        else
                        {
                            num = 1;
                        }

                        switch(c)
                        {
                            case '$':
                                currentPoint.X = 0;
                                for (int i = 0; i < num; i++)
                                {
                                    currentPoint.Y++;
                                }
                                break;
                            case 'b':
                            case '.':
                                for (int i = 0; i < num; i++)
                                {
                                    newGrid.PointPieces[currentPoint] = Piece.Get(0);
                                    currentPoint.X++;
                                }
                                break;
                            case 'o':
                            case 'A':
                                for (int i = 0; i < num; i++)
                                {
                                    newGrid.PointPieces[currentPoint] = Piece.Get(1);
                                    currentPoint.X++;
                                }
                                break;
                            case 'B':
                                for (int i = 0; i < num; i++)
                                {
                                    newGrid.PointPieces[currentPoint] = Piece.Get(2);
                                    currentPoint.X++;
                                }
                                break;
                            case '!':
                                break;
                        }
                    }

                    c = (char)reader.Read();
                }
                
            }
            return new CAPattern(newGrid, rule);

        }
    }

    public class CAPattern
    {
        public CAPattern(PieceGrid pattern, ICARule rule)
        {
            Pattern = pattern;
            Rule = rule;
        }

        public PieceGrid Pattern { get; private set; }
        public ICARule Rule { get; private set; }
    }
}
