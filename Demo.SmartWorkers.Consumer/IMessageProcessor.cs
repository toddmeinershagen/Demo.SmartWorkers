using Demo.SmartWorkers.Messages;

namespace Demo.SmartWorkers.Consumer
{
    public interface IMessageProcessor
    {
        bool Process(IPatientChanged message);
    }
}