using Demo.SmartWorkers.Core;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Demo.SmartWorkers.Consumer.UnitTests
{
    [TestFixture]
    public class when_determining_if_expiration_request
    {
        [Test]
        public void
            given_first_version_of_message_and_previous_version_exists_and_all_previous_versions_have_been_processed_should_return_true
            ()
        {
            var message = Substitute.For<IPatientChanged>();
            message.Version.Returns(1);
            message.PreviousVersion.Returns(217);

            var latestVersion = new PatientVersion { Version = 217 };
            message.IsExpirationRequest(latestVersion).Should().BeTrue();
        }

        [Test]
        public void
            given_first_version_of_message_and_previous_version_exists_but_not_all_previous_versions_have_been_processed_should_return_false
            ()
        {
            var message = Substitute.For<IPatientChanged>();
            message.Version.Returns(1);
            message.PreviousVersion.Returns(217);

            var latestVersion = new PatientVersion{Version = 215};
            message.IsExpirationRequest(latestVersion).Should().BeFalse();
        }

        [Test]
        public void given_first_version_of_message_and_previous_version_does_not_exist_should_return_false()
        {
            var message = Substitute.For<IPatientChanged>();
            message.Version.Returns(1);
            message.PreviousVersion.Returns(0);

            PatientVersion latestVersion = null;
            message.IsExpirationRequest(latestVersion).Should().BeFalse();
        }

        [Test]
        public void given_a_non_first_version_of_message_should_return_false()
        {
            var message = Substitute.For<IPatientChanged>();
            message.Version.Returns(3);
            message.PreviousVersion.Returns(2);

            PatientVersion latestVersion = null;
            message.IsExpirationRequest(latestVersion).Should().BeFalse();
        }
    }

    [TestFixture]
    public class when_determining_if_message_is_next_to_be_processed
    {
        [Test]
        public void given_first_version_of_message_and_no_previous_version_exists_should_return_true()
        {
            var message = Substitute.For<IPatientChanged>();
            message.Version.Returns(1);
            message.PreviousVersion.Returns(0);

            PatientVersion latestVersion = null;
            message.IsNextToBeProcessed(latestVersion).Should().BeTrue();
        }

        [Test]
        public void
            given_first_version_of_message_and_previous_version_exists_should_return_false
            ()
        {
            var message = Substitute.For<IPatientChanged>();
            message.Version.Returns(1);
            message.PreviousVersion.Returns(0);

            var latestVersion = new PatientVersion { Version = 2 };
            message.IsNextToBeProcessed(latestVersion).Should().BeFalse();
        }

        [Test]
        public void given_current_version_is_the_next_version_from_previous_version_should_return_true()
        {
            var message = Substitute.For<IPatientChanged>();
            message.Version.Returns(217);
            message.PreviousVersion.Returns(216);

            var latestVersion = new PatientVersion {Version = 216};
            message.IsNextToBeProcessed(latestVersion).Should().BeTrue();
        }

        [Test]
        public void given_current_version_is_not_the_next_version_from_previous_version_should_return_false()
        {
            var message = Substitute.For<IPatientChanged>();
            message.Version.Returns(217);
            message.PreviousVersion.Returns(216);

            var latestVersion = new PatientVersion { Version = 215 };
            message.IsNextToBeProcessed(latestVersion).Should().BeFalse();
        }
    }
}
