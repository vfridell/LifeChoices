using GameOfLifeLib.Models;
using GameOfLifeLib.Models.Games;
using System;
using System.Diagnostics;

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
            ToroidGameBase perfGame = new LifeGliderGunMix();
            perfGame.Initialize();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < framesToRun; i++)
            {
                perfGame.ExecuteGameLoop();
            }
            stopwatch.Stop();

            double framerate = framesToRun / (stopwatch.ElapsedMilliseconds / 1000d);
            Console.WriteLine($"{framesToRun} generations in {stopwatch.ElapsedMilliseconds / 1000d} seconds.");
            Console.WriteLine($"{framerate} generations per second");
        }
    }
}
