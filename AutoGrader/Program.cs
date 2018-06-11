using System;
using AutoGrader.Utils;

namespace AutoGrader
{
    internal class Program
    {
        private static void Main () {
            Serializer.InitializeDirectories();

            var cfg = new Configuration("config.json");

            var grader = new AutoGrader(cfg);
            grader.PrepareRoster();
            grader.PrepareSubmissions();
            grader.GradeSubmissions();

            Evaluater.PrintGradeCounts();

            Logger.Log("Time to upload doot doot motherfucker.");
            Console.Read();
            grader.UploadSubmissions();

            Logger.Log("Finished.");
            Console.Read();
        }
    }
}
