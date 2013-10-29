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
    public class MessageProcessorTests
    {
        private ILogger _logger;
        private IPatientChangedSnapshotRepository _repository;
        private MessageProcessor _processor;
        private IPatientChanged _message;

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger>();
            _repository = Substitute.For<IPatientChangedSnapshotRepository>();
            _processor = new MessageProcessor(_logger, _repository);

            _message = Substitute.For<IPatientChanged>();
            _message.MedicalRecordNumber.Returns(2);
        }

        [Test]
        public void given_repository_successfully_inserts_snapshot_when_processing_the_message_should_return_true()
        {
            _repository.Insert(Arg.Any<PatientChangedSnapshot>()).Returns(true);

            var result = _processor.Process(_message);

            result.Should().BeTrue();
        }

        [Test]
        public void given_repository_successfully_inserts_snapshot_when_processing_the_message_should_log_successful_message()
        {
            _repository.Insert(Arg.Any<PatientChangedSnapshot>()).Returns(true);

            _processor.Process(_message);

            var infoMessage = string.Format("Persisted context for MRN::2");
            _logger.Received(1).Info(infoMessage);
        }

        [Test]
        public void given_repository_unsuccessfully_inserts_snapshot_when_processing_the_message_should_return_false()
        {
            _repository.Insert(Arg.Any<PatientChangedSnapshot>()).Returns(false);

            var result = _processor.Process(_message);

            result.Should().BeFalse();
        }

        [Test]
        public void given_repository_unsuccessfully_inserts_snapshot_when_processing_the_message_should_not_log_successful_message()
        {
            _repository.Insert(Arg.Any<PatientChangedSnapshot>()).Returns(false);

            _processor.Process(_message);

            _logger.DidNotReceive().Info(Arg.Any<string>());
        }

        [Test]
        public void given_repository_throws_when_processing_the_message_should_throw()
        {
            var exception = new ArgumentException();
            _repository.Insert(Arg.Any<PatientChangedSnapshot>()).Returns(c => { throw exception; });

            Action action = () => _processor.Process(_message);

            action.ShouldThrow<ArgumentException>();
        }
    }
}
