using System;

namespace AutoGrader.Utils
{
    public static class Logger
    {
        public static void Log (object message) { Console.WriteLine(message.ToString()); }
    }
}
