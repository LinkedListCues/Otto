using System;

namespace AutoGrader
{
    internal class Program 
    {
        private static void Main(string[] args)
        {
            AutoGrader grader = new AutoGrader();
            grader.Initialize();

            Console.WriteLine("Hello World!");

            Console.Read();
        }
    }

    public interface IManager
    {
        void Initialize ();
    }
}
