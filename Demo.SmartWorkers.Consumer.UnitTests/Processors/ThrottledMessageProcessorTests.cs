using Demo.SmartWorkers.Consumer.Processors;
using Demo.SmartWorkers.Core;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Demo.SmartWorkers.Consumer.UnitTests.Processors
{
    [TestFixture]
    public class given_throttle_in_seconds_configured_and_inner_processor_succeeds_when_throttling
    {
        private IMessageProcessor _innerProcessor;
        private ThrottledMessageProcessor _processor;
        private int _milliseconds;
        private IPatientChanged _message;
        private bool _result;

        [SetUp]
        public void SetUp()
        {
            _innerProcessor = Substitute.For<IMessageProcessor>();
            _innerProcessor.Process(Arg.Any<IPatientChanged>()).Returns(true);

            _milliseconds = 0;
            _processor = new ThrottledMessageProcessor(_innerProcessor)
            {
                GetAppSetting = (x) => x == "throttleInSeconds" ? ".125" : null,
                SleepForMilliseconds = (throttleInMilliseconds) => _milliseconds = throttleInMilliseconds
            };

            _message = Substitute.For<IPatientChanged>();

            _result = _processor.Process(_message);
        }

        [Test]
        public void should_sleep_for_throttle()
        {
            _milliseconds.Should().Be(125);
        }

        [Test]
        public void should_return_true()
        {
            _result.Should().BeTrue();
        }
    }

    [TestFixture]
    public class given_throttle_in_seconds_configured_and_inner_processor_fails_when_throttling
    {
        private IMessageProcessor _innerProcessor;
        private ThrottledMessageProcessor _processor;
        private int _milliseconds;
        private IPatientChanged _message;
        private bool _result;

        [SetUp]
        public void SetUp()
        {
            _innerProcessor = Substitute.For<IMessageProcessor>();
            _innerProcessor.Process(Arg.Any<IPatientChanged>()).Returns(false);

            _milliseconds = 0;
            _processor = new ThrottledMessageProcessor(_innerProcessor)
            {
                GetAppSetting = (x) => x == "throttleInSeconds" ? ".125" : null,
                SleepForMilliseconds = (throttleInMilliseconds) => _milliseconds = throttleInMilliseconds
            };

            _message = Substitute.For<IPatientChanged>();

            _result = _processor.Process(_message);
        }

        [Test]
        public void should_sleep_for_throttle()
        {
            _milliseconds.Should().Be(125);
        }

        [Test]
        public void should_return_false()
        {
            _result.Should().BeFalse();
        }
    }
}
