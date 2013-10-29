using Demo.SmartWorkers.Core;

namespace Demo.SmartWorkers.Consumer.Processors
{
    public interface IMessageProcessor
    {
        bool Process(IPatientChanged message);
    }
}