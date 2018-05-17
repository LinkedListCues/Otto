using System;

namespace AutoGrader
{
    internal class Program
    {
        private static void Main (string[] args) {
            var grader = new AutoGrader();
            grader.Initialize();
            grader.DownloadSubmissions();
            //grader.GradeSubmissions();

            Logger.Log("Finished.");
            Console.Read();
        }
    }
}
