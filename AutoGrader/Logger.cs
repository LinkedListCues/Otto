using System;

namespace AutoGrader
{
    public static class Logger
    {
        public static void Log(string message) {
            Console.WriteLine(message);
        }

        public static void Log(object obj) {
            Console.WriteLine(obj.ToString());
        }
    }
}
