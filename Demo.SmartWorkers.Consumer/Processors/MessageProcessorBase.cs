using Demo.SmartWorkers.Core;

namespace Demo.SmartWorkers.Consumer.Processors
{
    public abstract class MessageProcessorBase
    {
        protected readonly ILogger Logger;

        protected MessageProcessorBase(ILogger logger)
        {
            Logger = logger;
        }
    }
}