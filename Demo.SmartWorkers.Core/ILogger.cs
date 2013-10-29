using System;

namespace Demo.SmartWorkers.Core
{
    public interface ILogger
    {
        void Info(string message);
        void Error(Exception exception);
    }
}
