using GameOfLifeLib;
using GameOfLifeLib.Models;
using GameOfLifeLib.Models.Games;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MeasurePerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            CalcFramerate();
        }


        private static void CalcFramerate()
        {
            int framesToRun = 1000;
            int maxSeconds = 100;
            //ToroidGameBase perfGame = new SeedsJustFriendsMix();
            //ToroidGameBase perfGame = new CoralGame();
            ToroidGameBase perfGame = new LifeMegaMix();
            //ToroidGameBase perfGame = new LifeGliderGunMix();
            perfGame.Initialize();
            int[] genPerSeconds = new int[maxSeconds];
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < framesToRun; i++)
            {
                perfGame.ExecuteGameLoop();
                genPerSeconds[(long)(stopwatch.ElapsedMilliseconds / 1000)]++;
            }
            stopwatch.Stop();

            double framerate = framesToRun / (stopwatch.ElapsedMilliseconds / 1000d);
            Console.WriteLine($"{framesToRun} generations in {stopwatch.ElapsedMilliseconds / 1000d} seconds.");
            Console.WriteLine($"{framerate} generations per second");
            double entropy = CalcEntropy(perfGame);
            Console.WriteLine($"Entropy: {entropy}");

            int j;
            StringBuilder ruler = new StringBuilder();
            for (j = 1; j < maxSeconds; j++)
            {
                if (j % 10 == 0) ruler.Append('+');
                else ruler.Append('-');
            }

            Console.WriteLine($"    {ruler}");
            j = 1;
            foreach (int generations in genPerSeconds)
            {
                Console.WriteLine($"{j++:D2}: " + new string('#', generations));
                if (generations == 0) break;
            }

        }


        private static double CalcEntropy(IHazGame game)
        {
            Dictionary<int, int> totalsPerState = new Dictionary<int, int>();
            foreach (Piece p in game.CurrentGeneration.PointPieces.Select(pp => pp.Value))
            {
                if (!totalsPerState.ContainsKey(p.StateValue)) totalsPerState.Add(p.StateValue, 0);
                totalsPerState[p.StateValue]++;
            }

            int totalCells = totalsPerState.Values.Sum();
            List<double> probabilities = new List<double>();
            double entropy = 0d;
            foreach (int value in totalsPerState.Keys)
            {
                double probability = (totalsPerState[value] / (double)totalCells);
                entropy +=  -((totalsPerState[value] * probability * Math.Log2(probability)) / Math.Log2(2));
            }
            return entropy;
        }
    }
}
