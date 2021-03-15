using MessageBus.Abstractions.Tests.Unit.Models.Commands;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageTests : MessageTestsBase
    {
        [Fact]
        public void CreatesNewMessageFromIEvent()
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Message<IEvent>(aircraftLandedEvent);

            Assert.Equal(aircraftLandedEvent.AircraftId, ((AircraftLanded)sut.Body).AircraftId);
            Assert.Equal(nameof(AircraftLanded), sut.Label);
        }
        
        [Fact]
        public void CreatesNewMessageFromICommand()
        {
            var command = BuildCreateNewFlightPlanCommand();

            var sut = new Message<ICommand>(command);

            Assert.Equal(command.Source, ((CreateNewFlightPlan)sut.Body).Source);
            Assert.Equal(nameof(CreateNewFlightPlan), sut.Label);
        }

        [Fact]
        public void CreatesNewIEventMessageFromString()
        {
            var eventString = Guid.NewGuid().ToString();

            var sut = new Message<IEvent>(eventString);

            Assert.Equal(eventString, sut.BodyAsString);
        }
        
        [Fact]
        public void CreatesNewICommandMessageFromString()
        {
            var eventString = Guid.NewGuid().ToString();

            var sut = new Message<ICommand>(eventString);

            Assert.Equal(eventString, sut.BodyAsString);
        }

        [Fact]
        public void CreatesNewEventMessageWithProperties()
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var correlationId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var messageProperties = BuildMessageProperties();

            var sut = new Message<IEvent>(aircraftLandedEvent)
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
        public void CreatesNewCommandMessageWithProperties()
        {
            var command = BuildCreateNewFlightPlanCommand();

            var correlationId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var messageProperties = BuildMessageProperties();

            var sut = new Message<ICommand>(command)
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
        public void CreatesNewEventMessageWithPropertiesUsingConstructor()
        {
            var correlationId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var messageProperties = BuildMessageProperties();

            var sut = new Message<IEvent>(BuildAircraftLandedEvent(), correlationId,
                messageId, messageProperties);

            Assert.Equal(correlationId, sut.CorrelationId);
            Assert.Equal(messageId, sut.MessageId);
            Assert.Equal(messageProperties, sut.MessageProperties);
        }
        
        [Fact]
        public void CreatesNewCommandMessageWithPropertiesUsingConstructor()
        {
            var correlationId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var messageProperties = BuildMessageProperties();

            var sut = new Message<ICommand>(BuildCreateNewFlightPlanCommand(), correlationId,
                messageId, messageProperties);

            Assert.Equal(correlationId, sut.CorrelationId);
            Assert.Equal(messageId, sut.MessageId);
            Assert.Equal(messageProperties, sut.MessageProperties);
        }

        [Fact]
        public void CreatesNewMessageFromIEventWithDefaultMessageIdAndCorrelationId()
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Message<IEvent>(aircraftLandedEvent);

            Assert.False(string.IsNullOrWhiteSpace(sut.MessageId));
        }

        [Fact]
        public void CreatesNewEventMessageFromStringWithDefaultMessageIdAndCorrelationId()
        {
            var eventString = Guid.NewGuid().ToString();

            var sut = new Message<IEvent>(eventString);

            Assert.False(string.IsNullOrWhiteSpace(sut.MessageId));
        }
        
        [Fact]
        public void CreatesNewMessageFromICommandWithDefaultMessageIdAndCorrelationId()
        {
            var command = BuildCreateNewFlightPlanCommand();

            var sut = new Message<ICommand>(command);

            Assert.False(string.IsNullOrWhiteSpace(sut.MessageId));
        }

        [Fact]
        public void CreatesNewCommandMessageFromStringWithDefaultMessageIdAndCorrelationId()
        {
            var eventString = Guid.NewGuid().ToString();

            var sut = new Message<ICommand>(eventString);

            Assert.False(string.IsNullOrWhiteSpace(sut.MessageId));
        }

        [Fact]
        public void AddsMessageVersionPropertyIfOverrideDefaultMessagePropertiesFalse()
        {
            var aircraftLandedEventV2 = new Models.Events.V2.AircraftLanded();

            var sut = new Message<IEvent>(aircraftLandedEventV2);
            sut.Build(new MessageBusOptions());

            Assert.Equal(2, int.Parse(sut.MessageProperties[_defaultMessageVersionPropertyName]));
        }
        
        [Fact]
        public void DoesNotAddMessageVersionPropertyIfNoMessageVersion()
        {
            var aircraftLandedEventV2 = new AircraftLanded();

            var sut = new Message<IEvent>(aircraftLandedEventV2);
            sut.Build(new MessageBusOptions());

            Assert.False(sut.MessageProperties.ContainsKey(_defaultMessageVersionPropertyName));
        }
        
        [Theory]
        [InlineData("MyMessageVersion")]
        [InlineData("Version")]
        public void AddsMessageCustomVersionPropertyIfOverrideDefaultMessagePropertiesFalse(string messageVersionPropertyName)
        {
            var aircraftLandedEventV2 = new Models.Events.V2.AircraftLanded();

            var sut = new Message<IEvent>(aircraftLandedEventV2);
            sut.Build(new MessageBusOptions { MessageVersionPropertyName = messageVersionPropertyName });

            Assert.Equal(2, int.Parse(sut.MessageProperties[messageVersionPropertyName]));
        }
        
        [Fact]
        public void DoesNotAddMessageVersionPropertyIfOverrideDefaultMessagePropertiesTrue()
        {
            var aircraftLandedEventV2 = new Models.Events.V2.AircraftLanded();

            var sut = new Message<IEvent>(aircraftLandedEventV2)
            {
                OverrideDefaultMessageProperties = true
            };

            sut.Build(new MessageBusOptions());

            Assert.False(sut.MessageProperties.ContainsKey(_defaultMessageVersionPropertyName));
        }
    }
}
