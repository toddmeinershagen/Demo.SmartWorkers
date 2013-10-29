using System;
using Demo.SmartWorkers.Consumer.Processors;
using Demo.SmartWorkers.Core;
using FluentAssertions;
using MassTransit;
using NSubstitute;
using NUnit.Framework;

namespace Demo.SmartWorkers.Consumer.UnitTests
{
    [TestFixture]
    public class PatientChangedConsumerTests
    {
        [Test]
        public void given_message_processor_that_succeeds_when_consuming_should_not_retry_message_later()
        {
            var logger = Substitute.For<ILogger>();
            var messageProcessor = Substitute.For<IMessageProcessor>();
            var context = Substitute.For<IConsumeContext<IPatientChanged>>();
            var message = Substitute.For<IPatientChanged>();
            context.Message.Returns(message);

            messageProcessor.Process(message).Returns(true);
            var consumer = new PatientChangedConsumer(logger, messageProcessor);

            consumer.Consume(context);

            context.DidNotReceive().RetryLater();
        }

        [Test]
        public void given_message_processor_that_does_not_succeed_when_consuming_should_retry_message_later()
        {
            var logger = Substitute.For<ILogger>();
            var messageProcessor = Substitute.For<IMessageProcessor>();
            var context = Substitute.For<IConsumeContext<IPatientChanged>>();
            var message = Substitute.For<IPatientChanged>();
            context.Message.Returns(message);

            messageProcessor.Process(message).Returns(false);
            var consumer = new PatientChangedConsumer(logger, messageProcessor);

            consumer.Consume(context);

            context.Received(1).RetryLater();
        }
    }

    public class given_message_processor_that_throws
    {
        private ILogger logger;
        private IConsumeContext<IPatientChanged> context;
        private Exception exception;
        private Action action;

        [SetUp]
        public void SetUp()
        {
            logger = Substitute.For<ILogger>();
            var messageProcessor = Substitute.For<IMessageProcessor>();
            context = Substitute.For<IConsumeContext<IPatientChanged>>();
            var message = Substitute.For<IPatientChanged>();
            context.Message.Returns(message);

            exception = new Exception("This is the message.");
            messageProcessor.Process(message).Returns((x) => { throw exception; });
            var consumer = new PatientChangedConsumer(logger, messageProcessor);

            action = () => consumer.Consume(context);
        }

        [Test]
        public void when_consuming_should_retry_message_later()
        {
            action();
            context.Received(1).RetryLater();
        }

        [Test]
        public void when_consuming_should_log_error()
        {
            action();
            logger.Received(1).Error(exception);
        }

        [Test]
        public void when_consuming_should_not_throw()
        {
            action.ShouldNotThrow<Exception>();
        }
    }
}
