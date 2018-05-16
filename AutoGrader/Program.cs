using System;

namespace AutoGrader
{
    internal class Program
    {
        private static void Main (string[] args) {
            var grader = new AutoGrader();
            grader.Initialize();
            grader.DownloadSubmissions();


            Logger.Log("Finished.");
            Console.Read();
        }
    }

    public interface IManager
    {
        void Initialize ();
    }
}
