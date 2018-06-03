using System;
using AutoGrader.Utils;

namespace AutoGrader
{
    internal class Program
    {
        private static void Main (string[] args) {
            Serializer.InitializeDirectories();

            var cfg = new Configuration("config.json");

            var grader = new AutoGrader(cfg);
            grader.PrepareRoster();
            grader.PrepareSubmissions();
            grader.GradeSubmissions();

            // todo take input before uploading
            grader.UploadSubmissions();

            Logger.Log("Finished.");
            Console.Read();
        }
    }
}
