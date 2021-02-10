using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class EventTests : MessageTestsBase
    {
        [Fact]
        public void CreatesNewMessageFromIEvent()
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Event(aircraftLandedEvent);

            Assert.Equal(aircraftLandedEvent.AircraftId, ((AircraftLanded)sut.Message).AircraftId);
        }

        [Fact]
        public void CreatesNewMessageFromString()
        {
            var eventString = Guid.NewGuid().ToString();

            var sut = new Event(eventString);

            Assert.Equal(eventString, sut.MessageAsString);
        }

        [Fact]
        public void CreatesNewMessageWithProperties()
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var correlationId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var messageProperties = BuildMessageProperties();

            var sut = new Event(aircraftLandedEvent)
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

            var sut = new Event(BuildAircraftLandedEvent(), correlationId,
                messageId, messageProperties);

            Assert.Equal(correlationId, sut.CorrelationId);
            Assert.Equal(messageId, sut.MessageId);
            Assert.Equal(messageProperties, sut.MessageProperties);
        }

        [Fact]
        public void CreatesNewMessageFromIEventWithDefaultMessageIdAndCorrelationId()
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Event(aircraftLandedEvent);

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
