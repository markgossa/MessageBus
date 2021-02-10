using MessageBus.Abstractions.Tests.Unit.Models.Commands;
using System;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class CommandTests : MessageTestsBase
    {
        [Fact]
        public void CreatesNewMessageFromICommand()
        {
            var createNewFlightPlanCommand = BuildCreateNewFlightPlan();

            var sut = new Command(createNewFlightPlanCommand);

            Assert.Equal(createNewFlightPlanCommand.Source, ((CreateNewFlightPlan)sut.Message).Source);
        }

        [Fact]
        public void CreatesNewMessageFromString()
        {
            var eventString = Guid.NewGuid().ToString();

            var sut = new Event(eventString);

            Assert.Equal(eventString, sut.MessageAsString);
        }

        [Fact]
        public void CreatesNewMessageWithPropertiesUsingPropertyInitializer()
        {
            var correlationId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var messageProperties = BuildMessageProperties();

            var sut = new Command(BuildCreateNewFlightPlan())
            {
                CorrelationId = correlationId,
                MessageId = messageId,
                MessageProperties = messageProperties
            };

            Assert.Equal(correlationId, sut.CorrelationId);
            Assert.Equal(messageId, sut.MessageId);
            Assert.Equal(messageProperties, sut.MessageProperties);
        }

        [Fact]
        public void CreatesNewMessageWithPropertiesUsingConstructor()
        {
            var correlationId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var messageProperties = BuildMessageProperties();

            var sut = new Command(BuildCreateNewFlightPlan(), correlationId,
                messageId, messageProperties);

            Assert.Equal(correlationId, sut.CorrelationId);
            Assert.Equal(messageId, sut.MessageId);
            Assert.Equal(messageProperties, sut.MessageProperties);
        }

        [Fact]
        public void CreatesNewMessageFromIEventWithDefaultMessageIdAndCorrelationId()
        {
            var sut = new Command(BuildCreateNewFlightPlan());

            Assert.False(string.IsNullOrWhiteSpace(sut.CorrelationId));
            Assert.False(string.IsNullOrWhiteSpace(sut.MessageId));
        }

        [Fact]
        public void CreatesNewMessageFromStringWithDefaultMessageIdAndCorrelationId()
        {
            var eventString = Guid.NewGuid().ToString();

            var sut = new Event(eventString);

            Assert.False(string.IsNullOrWhiteSpace(sut.CorrelationId));
            Assert.False(string.IsNullOrWhiteSpace(sut.MessageId));
        }
    }
}
