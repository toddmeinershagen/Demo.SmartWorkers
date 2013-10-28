using System;
using System.Configuration;
using System.Threading;
using Demo.SmartWorkers.Data;
using Demo.SmartWorkers.Messages;
using MassTransit;

namespace Demo.SmartWorkers.Consumer
{
    public class PatientChangedConsumer : Consumes<IPatientChanged>.Context
    {
        private readonly IPatientVersionRepository _patientVersionRepository;
        private readonly IMessageProcessor _messageProcessor;

        public PatientChangedConsumer()
            : this(new PatientVersionRepository("patientVersionForConsumer"), new MessageProcessor(new PatientChangedSnapshotRepository()))
        {}

        public PatientChangedConsumer(IPatientVersionRepository patientVersionRepository, IMessageProcessor messageProcessor)
        {
            _patientVersionRepository = patientVersionRepository;
            _messageProcessor = messageProcessor;
        }

        public void Consume(IConsumeContext<IPatientChanged> context)
        {
            var throttleInSeconds = Convert.ToDouble(ConfigurationManager.AppSettings["throttleInSeconds"]);
            Throttle(throttleInSeconds);

            var message = context.Message;
                
            try
            {
                var patientVersion = _patientVersionRepository.FindOne(message.FacilityId, message.MedicalRecordNumber);
                if ((DoesNotExist(patientVersion)  && (message.Version == 1)) || IsNextVersion(patientVersion, message))
                {
                    if (_messageProcessor.Process(message))
                    {
                        _patientVersionRepository.Increment(message.FacilityId, message.MedicalRecordNumber);
                        Console.WriteLine("Persisted context for MRN::{0}", message.MedicalRecordNumber);
                    }
                }
                else
                {
                    context.RetryLater();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.RetryLater();
            }
        }

        private bool IsNextVersion(PatientVersion patientVersion, IPatientChanged message)
        {
            return patientVersion.Version == message.Version - 1;
        }

        private bool DoesNotExist(PatientVersion patientVersion)
        {
            return patientVersion == null;
        }

        private void Throttle(double seconds)
        {
            var throttleSeconds = Convert.ToInt32(Math.Round(seconds*1000, 0));
            Thread.Sleep(throttleSeconds);
        }
    }
}