using System;
using Demo.SmartWorkers.Consumer.Processors;
using Demo.SmartWorkers.Core;
using Demo.SmartWorkers.Data;
using MassTransit;

namespace Demo.SmartWorkers.Consumer
{
    public class PatientChangedConsumer : Consumes<IPatientChanged>.Context
    {
        private readonly ILogger _logger;
        private readonly IMessageProcessor _messageProcessor;

        public PatientChangedConsumer()
            : this(new ConsoleLogger())
        { }

        public PatientChangedConsumer(ILogger logger)
            : this(logger,
            new ThrottledMessageProcessor(
                new VersionedMessageProcessor(
                    new MessageProcessor(logger, new PatientChangedSnapshotRepository()), new PatientVersionRepository("patientVersionForConsumer"))))
        { }

        //NOTE:  This version is with the locking decorator.  It is no longer needed, but here for demonstration purposes.
        //public PatientChangedConsumer(ILogger logger)
        //    : this(logger, 
        //    new ThrottledMessageProcessor(
        //        new LockedMessageProcessor(
        //            new VersionedMessageProcessor(
        //                new MessageProcessor(logger, new PatientChangedSnapshotRepository()), new PatientVersionRepository("patientVersionForConsumer")), new PatientLockRepository())))
        //{}

        public PatientChangedConsumer(ILogger logger, IMessageProcessor messageProcessor)
        {
            _logger = logger;
            _messageProcessor = messageProcessor;
        }

        public void Consume(IConsumeContext<IPatientChanged> context)
        {
            var message = context.Message;
                
            try
            {
                if(!_messageProcessor.Process(message))
                {
                    context.RetryLater();
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
                context.RetryLater();
            }
        }
    }
}