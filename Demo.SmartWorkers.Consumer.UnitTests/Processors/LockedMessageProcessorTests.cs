using System;
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
    public class given_expiration_in_seconds_configured_and_lock_does_not_exist_and_inner_process_succeeds_when_processing
    {
        const int FacilityId = 1;
        const int MedicalRecordNumber = 12700;
        private IPatientLockRepository _patientLockRepository;
        private bool _result;

        [SetUp]
        public void SetUp()
        {
            var innerProcessor = Substitute.For<IMessageProcessor>();
            var message = Substitute.For<IPatientChanged>();
            message.FacilityId.Returns(FacilityId);
            message.MedicalRecordNumber.Returns(MedicalRecordNumber);

            innerProcessor.Process(message).Returns(true);

            _patientLockRepository = Substitute.For<IPatientLockRepository>();
            _patientLockRepository.DoesNotExistFor(FacilityId, MedicalRecordNumber).Returns(true);

            var processor = new LockedMessageProcessor(innerProcessor, _patientLockRepository);
            processor.GetAppSetting = (x) => x == "expirationInMinutes" ? "1" : null;

            _result = processor.Process(message);
        }

        [Test]
        public void should_return_true()
        {
            _result.Should().BeTrue();
        }

        [Test]
        public void should_insert_lock()
        {
            _patientLockRepository.Received(1).Insert(Arg.Any<PatientLock>());
        }

        [Test]
        public void should_remove_lock()
        {
            _patientLockRepository.Received(1).Remove(FacilityId, MedicalRecordNumber);
        }
    }

    [TestFixture]
    public class given_expiration_in_seconds_configured_and_lock_does_not_exist_and_inner_process_fails_when_processing
    {
        const int FacilityId = 1;
        const int MedicalRecordNumber = 12700;
        private IPatientLockRepository _patientLockRepository;
        private bool _result;

        [SetUp]
        public void SetUp()
        {
            var innerProcessor = Substitute.For<IMessageProcessor>();
            var message = Substitute.For<IPatientChanged>();
            message.FacilityId.Returns(FacilityId);
            message.MedicalRecordNumber.Returns(MedicalRecordNumber);

            innerProcessor.Process(message).Returns(false);

            _patientLockRepository = Substitute.For<IPatientLockRepository>();
            _patientLockRepository.DoesNotExistFor(FacilityId, MedicalRecordNumber).Returns(true);

            var processor = new LockedMessageProcessor(innerProcessor, _patientLockRepository);
            processor.GetAppSetting = (x) => x == "expirationInMinutes" ? "1" : null;

            _result = processor.Process(message);
        }

        [Test]
        public void should_return_false()
        {
            _result.Should().BeFalse();
        }

        [Test]
        public void should_insert_lock()
        {
            _patientLockRepository.Received(1).Insert(Arg.Any<PatientLock>());
        }

        [Test]
        public void should_remove_lock()
        {
            _patientLockRepository.Received(1).Remove(FacilityId, MedicalRecordNumber);
        }
    }

    [TestFixture]
    public class given_expiration_in_seconds_configured_and_lock_exists_succeeds_when_processing
    {
        const int FacilityId = 1;
        const int MedicalRecordNumber = 12700;
        private IMessageProcessor _innerProcessor;
        private IPatientLockRepository _patientLockRepository;
        private bool _result;

        [SetUp]
        public void SetUp()
        {
            _innerProcessor = Substitute.For<IMessageProcessor>();
            var message = Substitute.For<IPatientChanged>();
            message.FacilityId.Returns(FacilityId);
            message.MedicalRecordNumber.Returns(MedicalRecordNumber);

            _patientLockRepository = Substitute.For<IPatientLockRepository>();
            _patientLockRepository.DoesNotExistFor(FacilityId, MedicalRecordNumber).Returns(false);

            var processor = new LockedMessageProcessor(_innerProcessor, _patientLockRepository);
            processor.GetAppSetting = (x) => x == "expirationInMinutes" ? "1" : null;

            _result = processor.Process(message);
        }

        [Test]
        public void should_return_false()
        {
            _result.Should().BeFalse();
        }

        [Test]
        public void should_not_insert_lock()
        {
            _patientLockRepository.DidNotReceive().Insert(Arg.Any<PatientLock>());
        }

        [Test]
        public void should_not_remove_lock()
        {
            _patientLockRepository.DidNotReceive().Remove(FacilityId, MedicalRecordNumber);
        }

        [Test]
        public void should_not_process_message()
        {
            _innerProcessor.DidNotReceive().Process(Arg.Any<IPatientChanged>());
        }
    }

    [TestFixture]
    public class given_expiration_in_seconds_configured_and_lock_does_not_exist_and_insertion_of_lock_throws_when_processing
    {
        const int FacilityId = 1;
        const int MedicalRecordNumber = 12700;
        private IMessageProcessor _innerProcessor;
        private IPatientLockRepository _patientLockRepository;
        private IPatientChanged _message;
        private LockedMessageProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            _message = Substitute.For<IPatientChanged>();
            _message.FacilityId.Returns(FacilityId);
            _message.MedicalRecordNumber.Returns(MedicalRecordNumber);

            _innerProcessor = Substitute.For<IMessageProcessor>();
            _innerProcessor.Process(_message).Returns(true);

            _patientLockRepository = Substitute.For<IPatientLockRepository>();
            _patientLockRepository.DoesNotExistFor(FacilityId, MedicalRecordNumber).Returns(true);
            _patientLockRepository.When(x => x.Insert(Arg.Any<PatientLock>())).Do((x => { throw new ArgumentException(); }));

            _processor = new LockedMessageProcessor(_innerProcessor, _patientLockRepository);
            _processor.GetAppSetting = (x) => x == "expirationInMinutes" ? "1" : null;
        }

        [Test]
        public void should_throw()
        {
            Action action = () => _processor.Process(_message);
            action.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void should_not_process_message()
        {
            Gulp(() => _processor.Process(_message));
            _innerProcessor.DidNotReceive().Process(_message);
        }

        [Test]
        public void should_not_remove_lock()
        {
            Gulp(() => _processor.Process(_message));
            _patientLockRepository.DidNotReceive().Remove(FacilityId, MedicalRecordNumber);
        }

        private void Gulp(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            { }
        }
    }
}
