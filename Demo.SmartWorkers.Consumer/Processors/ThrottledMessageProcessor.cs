using System;
using System.Configuration;
using System.Threading;
using Demo.SmartWorkers.Core;

namespace Demo.SmartWorkers.Consumer.Processors
{
    public class ThrottledMessageProcessor : IMessageProcessor
    {
        private readonly IMessageProcessor _messageProcessor;

        public ThrottledMessageProcessor(IMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
            GetAppSetting = (name) => ConfigurationManager.AppSettings[name];
            SleepForMilliseconds = Thread.Sleep;
        }

        public bool Process(IPatientChanged message)
        {
            Throttle();
            return _messageProcessor.Process(message);
        }

        private void Throttle()
        {
            var throttleInSeconds = Convert.ToDouble(GetAppSetting("throttleInSeconds"));
            var throttleInMilliseconds = Convert.ToInt32(Math.Round(throttleInSeconds*1000, 0));
            SleepForMilliseconds(throttleInMilliseconds);
        }

        public Func<string, string> GetAppSetting { get; set; }
        public Action<int> SleepForMilliseconds { get; set; } 
    }
}