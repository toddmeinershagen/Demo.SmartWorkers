using Demo.SmartWorkers.Consumer.Processors;
using Demo.SmartWorkers.Core;
using Demo.SmartWorkers.Core.Data;
using Demo.SmartWorkers.Data;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Demo.SmartWorkers.Consumer.UnitTests.Processors
{
    [TestFixture]
    public class given_no_previous_version_exists_and_version_of_message_is_one_and_inner_processor_is_successful_when_processing : VersionedMessageProcessorTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            VersionRepository = Substitute.For<IPatientVersionRepository>();
            VersionRepository.FindOne(FacilityId, MedicalRecordNumber).Returns(null as PatientVersion);

            var innerProcessor = Substitute.For<IMessageProcessor>();
            var message = Substitute.For<IPatientChanged>();
            message.FacilityId.Returns(FacilityId);
            message.MedicalRecordNumber.Returns(MedicalRecordNumber);
            message.Version.Returns(1);

            innerProcessor.Process(message).Returns(true);

            var processor = new VersionedMessageProcessor(innerProcessor, VersionRepository);
            Result = processor.Process(message);
        }

        [Test]
        public void should_increment_version()
        {
            VersionRepository.Received(1).Increment(FacilityId, MedicalRecordNumber);
        }

        [Test]
        public void should_return_true()
        {
            Result.Should().BeTrue();
        }    
    }

    [TestFixture]
    public class given_no_previous_version_exists_and_version_of_message_is_one_and_inner_processor_is_unsuccessful_when_processing : VersionedMessageProcessorTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            VersionRepository = Substitute.For<IPatientVersionRepository>();
            VersionRepository.FindOne(FacilityId, MedicalRecordNumber).Returns(null as PatientVersion);

            var innerProcessor = Substitute.For<IMessageProcessor>();
            var message = Substitute.For<IPatientChanged>();
            message.FacilityId.Returns(FacilityId);
            message.MedicalRecordNumber.Returns(MedicalRecordNumber);
            message.Version.Returns(1);

            innerProcessor.Process(message).Returns(false);

            var processor = new VersionedMessageProcessor(innerProcessor, VersionRepository);
            Result = processor.Process(message);
        }

        [Test]
        public void should_not_increment_version()
        {
            VersionRepository.DidNotReceive().Increment(FacilityId, MedicalRecordNumber);
        }

        [Test]
        public void should_return_false()
        {
            Result.Should().BeFalse();
        }
    }

    [TestFixture]
    public class given_no_previous_version_exists_and_version_of_message_is_greater_than_one_when_processing : VersionedMessageProcessorTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            VersionRepository = Substitute.For<IPatientVersionRepository>();
            VersionRepository.FindOne(FacilityId, MedicalRecordNumber).Returns(null as PatientVersion);

            var innerProcessor = Substitute.For<IMessageProcessor>();
            var message = Substitute.For<IPatientChanged>();
            message.FacilityId.Returns(FacilityId);
            message.MedicalRecordNumber.Returns(MedicalRecordNumber);
            message.Version.Returns(2);

            var processor = new VersionedMessageProcessor(innerProcessor, VersionRepository);
            processor.Process(message);
        }

        [Test]
        public void should_not_increment_version()
        {
            VersionRepository.DidNotReceive().Increment(FacilityId, MedicalRecordNumber);
        }

        [Test]
        public void should_return_true()
        {
            Result.Should().BeFalse();
        }
    }

    public class given_previous_version_exists_and_version_of_message_is_next_version_and_inner_processor_is_successful_when_processing : VersionedMessageProcessorTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            VersionRepository = Substitute.For<IPatientVersionRepository>();
            var patientVersion = new PatientVersion
                {
                    FacilityId = FacilityId,
                    MedicalRecordNumber = MedicalRecordNumber,
                    Version = 3
                };

            VersionRepository.FindOne(FacilityId, MedicalRecordNumber).Returns(patientVersion);

            var innerProcessor = Substitute.For<IMessageProcessor>();
            var message = Substitute.For<IPatientChanged>();
            message.FacilityId.Returns(FacilityId);
            message.MedicalRecordNumber.Returns(MedicalRecordNumber);
            message.Version.Returns(4);

            innerProcessor.Process(message).Returns(true);

            var processor = new VersionedMessageProcessor(innerProcessor, VersionRepository);
            Result = processor.Process(message);
        }

        [Test]
        public void should_increment_version()
        {
            VersionRepository.Received(1).Increment(FacilityId, MedicalRecordNumber);
        }

        [Test]
        public void should_return_true()
        {
            Result.Should().BeTrue();
        }
    }

    public class given_previous_version_exists_and_version_of_message_is_next_version_and_inner_processor_is_unsuccessful_when_processing : VersionedMessageProcessorTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            const int currentVersion = 3;
            VersionRepository = Substitute.For<IPatientVersionRepository>();
            var patientVersion = new PatientVersion
                {
                    FacilityId = FacilityId,
                    MedicalRecordNumber = MedicalRecordNumber,
                    Version = currentVersion
                };

            VersionRepository.FindOne(FacilityId, MedicalRecordNumber).Returns(patientVersion);

            var innerProcessor = Substitute.For<IMessageProcessor>();
            var message = Substitute.For<IPatientChanged>();
            message.FacilityId.Returns(FacilityId);
            message.MedicalRecordNumber.Returns(MedicalRecordNumber);
            message.Version.Returns(currentVersion + 1);

            innerProcessor.Process(message).Returns(false);

            var processor = new VersionedMessageProcessor(innerProcessor, VersionRepository);
            Result = processor.Process(message);
        }

        [Test]
        public void should_not_increment_version()
        {
            VersionRepository.DidNotReceive().Increment(FacilityId, MedicalRecordNumber);
        }

        [Test]
        public void should_return_false()
        {
            Result.Should().BeFalse();
        }
    }

    public class given_previous_version_exists_and_version_of_message_is_not_next_version_when_processing : VersionedMessageProcessorTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            const int currentVersion = 3;
            VersionRepository = Substitute.For<IPatientVersionRepository>();
            var patientVersion = new PatientVersion
            {
                FacilityId = FacilityId,
                MedicalRecordNumber = MedicalRecordNumber,
                Version = currentVersion
            };

            VersionRepository.FindOne(FacilityId, MedicalRecordNumber).Returns(patientVersion);

            var innerProcessor = Substitute.For<IMessageProcessor>();
            var message = Substitute.For<IPatientChanged>();
            message.FacilityId.Returns(FacilityId);
            message.MedicalRecordNumber.Returns(MedicalRecordNumber);
            message.Version.Returns(currentVersion + 2);

            var processor = new VersionedMessageProcessor(innerProcessor, VersionRepository);
            Result = processor.Process(message);
        }

        [Test]
        public void should_not_increment_version()
        {
            VersionRepository.DidNotReceive().Increment(FacilityId, MedicalRecordNumber);
        }

        [Test]
        public void should_return_false()
        {
            Result.Should().BeFalse();
        }
    }

    public abstract class VersionedMessageProcessorTestsBase
    {
        protected const int FacilityId = 1;
        protected const int MedicalRecordNumber = 12700;
        protected IPatientVersionRepository VersionRepository;
        protected bool Result;   
    }
}
