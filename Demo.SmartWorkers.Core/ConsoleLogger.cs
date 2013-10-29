using System;

namespace Demo.SmartWorkers.Core
{
    public class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            const string format = "INFO - {0}";
            Console.WriteLine(format, message);
        }

        public void Error(Exception exception)
        {
            const string format = "ERROR - {0}";
            Console.WriteLine(format, exception.Message);
        }
    }
}