using Demo.SmartWorkers.Core;
using Demo.SmartWorkers.Core.Data;
using MassTransit;
using NSubstitute;
using NUnit.Framework;

namespace Demo.SmartWorkers.Publisher.Tests
{
    [TestFixture]
    public class PublisherServiceTests
    {
        const int VersionExpirationInMinutes = 1;
        const int ThrottleInSeconds = 0;
        private PublisherService _publisher;
        private IPatientVersionRepository _repository;
        private IServiceBus _bus;

        [SetUp]
        public void SetUp()
        {
            var logger = Substitute.For<ILogger>();
            _repository = Substitute.For<IPatientVersionRepository>();
            _bus = Substitute.For<IServiceBus>();

            _publisher = new PublisherService(logger, _repository, _bus)
            {
                GetAppSetting = (name) =>
                {
                    if (name == "versionExpirationInMinutes")
                        return VersionExpirationInMinutes.ToString();

                    if (name == "throttleInSeconds")
                        return ThrottleInSeconds.ToString();

                    return null;
                }
            };
        }

        [Test]
        public void given_one_patient_changed_message_and_publish_once_when_publishing_should_publish_message_with_correct_versions_once()
        {
            const int facilityId = 1;
            const int medicalRecordNumber = 12700;
            const int previousVersion = 127;
            const int currentVersion = 1;
            _repository.RemoveIfExpired(facilityId, medicalRecordNumber, VersionExpirationInMinutes).Returns(previousVersion);
            _repository.Increment(facilityId, medicalRecordNumber).Returns(currentVersion);

            var message = new PatientChanged {FacilityId = facilityId, MedicalRecordNumber = medicalRecordNumber, Version = 1, PreviousVersion = 0};
            _publisher.Publish(1, new[]{message});

            _bus.Received(1).Publish(Arg.Is<PatientChanged>(x => 
                (x.FacilityId == facilityId) 
                && (x.MedicalRecordNumber == medicalRecordNumber)
                && (x.Version == currentVersion)
                && (x.PreviousVersion == previousVersion)));
        }

        [Test]
        public void given_one_patient_changed_message_and_publish_multiple_times_when_publishing_should_publish_message_with_correct_versions_multiple_times()
        {
            const int facilityId = 1;
            const int medicalRecordNumber = 12700;
            const int previousVersion = 127;
            const int currentVersion = 1;
            _repository.RemoveIfExpired(facilityId, medicalRecordNumber, VersionExpirationInMinutes).Returns(previousVersion);
            _repository.Increment(facilityId, medicalRecordNumber).Returns(currentVersion);

            var message = new PatientChanged { FacilityId = facilityId, MedicalRecordNumber = medicalRecordNumber, Version = 1, PreviousVersion = 0 };
            const int multipleTimes = 10;
            _publisher.Publish(multipleTimes, new[] { message });

            _bus.Received(multipleTimes).Publish(Arg.Is<PatientChanged>(x =>
                (x.FacilityId == facilityId)
                && (x.MedicalRecordNumber == medicalRecordNumber)
                && (x.Version == currentVersion)
                && (x.PreviousVersion == previousVersion)));
        }

        [Test]
        public void given_multiple_patient_changed_messages_and_publish_multiple_times_when_publishing_should_publish_messages_with_correct_versions_multiple_times()
        {
            const int previousVersion = 127;
            const int currentVersion = 1;
            _repository.RemoveIfExpired(Arg.Any<int>(), Arg.Any<int>(), VersionExpirationInMinutes).Returns(previousVersion);
            _repository.Increment(Arg.Any<int>(), Arg.Any<int>()).Returns(currentVersion);

            var message1 = new PatientChanged { FacilityId = 1, MedicalRecordNumber = 12700, Version = 1, PreviousVersion = 0 };
            var message2 = new PatientChanged { FacilityId = 1, MedicalRecordNumber = 12701, Version = 1, PreviousVersion = 0 };
            var messages = new[] {message1, message2};

            var counter = 0;
            _publisher.GetNextRandomNumber = (min, max) => GetAlternatingNumber(ref counter);

            const int multipleTimes = 10;
            _publisher.Publish(multipleTimes, messages);

            _bus.Received(multipleTimes / 2).Publish(Arg.Is<PatientChanged>(x =>
                (x.FacilityId == message1.FacilityId)
                && (x.MedicalRecordNumber == message1.MedicalRecordNumber)
                && (x.Version == currentVersion)
                && (x.PreviousVersion == previousVersion)));

            _bus.Received(multipleTimes / 2).Publish(Arg.Is<PatientChanged>(x =>
                (x.FacilityId == message2.FacilityId)
                && (x.MedicalRecordNumber == message2.MedicalRecordNumber)
                && (x.Version == currentVersion)
                && (x.PreviousVersion == previousVersion)));
        }

        private int GetAlternatingNumber(ref int counter)
        {
            return ++counter % 2;
        }
    }
}
