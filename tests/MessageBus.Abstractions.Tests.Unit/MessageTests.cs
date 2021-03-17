using MessageBus.Abstractions.Tests.Unit.Models.Commands;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using System;
using System.Collections.Generic;
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
        }

        [Fact]
        public void CreatesNewMessageFromICommand()
        {
            var command = BuildCreateNewFlightPlanCommand();

            var sut = new Message<ICommand>(command);

            Assert.Equal(command.Source, ((CreateNewFlightPlan)sut.Body).Source);
        }

        [Fact]
        public void CreatesNewIEventMessageFromString()
        {
            var eventString = Guid.NewGuid().ToString();

            var sut = new Message<IEvent>(eventString, "MyLabel");

            Assert.Equal(eventString, sut.BodyAsString);
        }

        [Fact]
        public void CreatesNewICommandMessageFromString()
        {
            var eventString = Guid.NewGuid().ToString();

            var sut = new Message<ICommand>(eventString, "MyLabel");

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

            var sut = new Message<IEvent>(eventString, "MyLabel");

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

            var sut = new Message<ICommand>(eventString, "MyLabel");

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
            Assert.Empty(sut.MessageProperties);
        }

        [Fact]
        public void AddsCustomMessagePropertiesIfOverrideDefaultMessagePropertiesTrue()
        {
            var aircraftLandedEventV2 = new Models.Events.V2.AircraftLanded();

            var customMessageProperties = new Dictionary<string, string>
                {
                    { "Property1", "1" },
                    { "Property2", "2" }
                };

            var sut = new Message<IEvent>(aircraftLandedEventV2)
            {
                OverrideDefaultMessageProperties = true,
                MessageProperties = customMessageProperties
            };

            sut.Build(new MessageBusOptions());

            Assert.Equal(2, sut.MessageProperties.Count);
            Assert.Equal(customMessageProperties, sut.MessageProperties);
        }

        [Fact]
        public void AddsCustomMessagePropertiesAndMessageVersionIfOverrideDefaultMessagePropertiesFalse()
        {
            var aircraftLandedEventV2 = new Models.Events.V2.AircraftLanded();

            var customMessageProperties = new Dictionary<string, string>
                {
                    { "Property1", "1" },
                    { "Property2", "2" }
                };

            var sut = new Message<IEvent>(aircraftLandedEventV2)
            {
                OverrideDefaultMessageProperties = false,
                MessageProperties = customMessageProperties
            };

            sut.Build(new MessageBusOptions());

            Assert.Equal(3, sut.MessageProperties.Count);
            Assert.Equal(customMessageProperties, sut.MessageProperties);
            Assert.Equal(2, int.Parse(sut.MessageProperties[_defaultMessageVersionPropertyName]));
        }

        [Theory]
        [InlineData("Mylabel")]
        [InlineData("Mylabel2")]
        public void LabelReturnsLabelIfLabelSpecified(string label)
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Message<IEvent>(aircraftLandedEvent)
            {
                Label = label
            };

            Assert.Equal(label, sut.Label);
        }

        [Fact]
        public void LabelReturnsTheNameOfTheTypeOfTheMessageIfNoLabelOrMessageTypePropertySpecified()
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Message<IEvent>(aircraftLandedEvent);

            Assert.Equal(nameof(AircraftLanded), sut.Label);
        }

        [Theory]
        [InlineData("MyAircraftLanded")]
        [InlineData("AnotherAircraftLanded")]
        public void LabelReturnsNullIfMessageTypePropertySpecified(string messageType)
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Message<IEvent>(aircraftLandedEvent)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", messageType }
                }
            };

            sut.Build(new());

            Assert.True(string.IsNullOrWhiteSpace(sut.Label));
            Assert.Equal(messageType, sut.MessageProperties[_defaultMessageTypePropertyName]);
        }

        [Theory]
        [InlineData("MyMessageType", "MyAircraftLanded")]
        [InlineData("MyMessageType123", "AnotherAircraftLanded")]
        public void LabelReturnsNullIfCustomMessageTypePropertySpecified(string messageTypePropertyName,
            string messageType)
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Message<IEvent>(aircraftLandedEvent)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { messageTypePropertyName, messageType }
                }
            };

            sut.Build(new MessageBusOptions { MessageTypePropertyName = messageTypePropertyName });

            Assert.True(string.IsNullOrWhiteSpace(sut.Label));
            Assert.Equal(messageType, sut.MessageProperties[messageTypePropertyName]);
        }

        [Theory]
        [InlineData("MyMessageType", "")]
        [InlineData("MyMessageType123", " ")]
        [InlineData("MyMessageType123", null)]
        public void LabelReturnsTypeOfMessageIfCustomMessageTypePropertySpecifiedButIsEmpty(string messageTypePropertyName,
            string messageType)
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Message<IEvent>(aircraftLandedEvent)
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { messageTypePropertyName, messageType }
                }
            };

            sut.Build(new MessageBusOptions { MessageTypePropertyName = messageTypePropertyName });

            Assert.Equal(nameof(AircraftLanded), sut.Label);
            Assert.Equal(messageType, sut.MessageProperties[messageTypePropertyName]);
        }

        [Theory]
        [InlineData("mylabel", "AircraftLandedFine")]
        [InlineData("Label123", "AircraftHadBumpyLanding")]
        public void LabelReturnsLabelIfBothLabelAndMessageTypeSpecified(string label,
            string messageType)
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Message<IEvent>(aircraftLandedEvent)
            {
                Label = label,
                MessageProperties = new()
                {
                    { "MessageType", messageType }
                }
            };

            Assert.Equal(messageType, sut.MessageProperties[_defaultMessageTypePropertyName]);
            Assert.Equal(label, sut.Label);
        }

        [Theory]
        [InlineData("mylabel", "AircraftLandedFine", "MyMessageType")]
        [InlineData("Label123", "AircraftHadBumpyLanding", "MessageIdentifier")]
        public void LabelReturnsLabelIfBothLabelAndCustomMessageTypeSpecified(string label,
            string messageType, string messageTypePropertyName)
        {
            var aircraftLandedEvent = BuildAircraftLandedEvent();

            var sut = new Message<IEvent>(aircraftLandedEvent)
            {
                Label = label,
                MessageProperties = new()
                {
                    { messageTypePropertyName, messageType }
                }
            };

            Assert.Equal(messageType, sut.MessageProperties[messageTypePropertyName]);
            Assert.Equal(label, sut.Label);
        }

        [Fact]
        public void ThrowsIfMessageFromStringAndLabelAndMessageTypeNotSpecified()
        {
            var eventString = Guid.NewGuid().ToString();
            var sut = new Message<IEvent>(eventString, null);

            Assert.Throws<ArgumentNullException>(() => sut.Build(new()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsIfMessageFromStringAndLabelEmpty(string label)
        {
            var eventString = Guid.NewGuid().ToString();
            var sut = new Message<IEvent>(eventString, label);

            Assert.Throws<ArgumentNullException>(() => sut.Build(new()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsIfMessageFromStringAndMessageTypeEmpty(string messageType)
        {
            var eventString = Guid.NewGuid().ToString();
            var customMessageProperties = new Dictionary<string, string>() { { _defaultMessageTypePropertyName, messageType } };
            var sut = new Message<IEvent>(eventString, null) { MessageProperties = customMessageProperties };

            Assert.Throws<ArgumentNullException>(() => sut.Build(new()));
        }

        [Fact]
        public void DoesNotThrowIfMessageFromStringAndOnlyLabelSpecified()
        {
            var eventString = Guid.NewGuid().ToString();
            var sut = new Message<IEvent>(eventString, "StringMessage");
            sut.Build(new());

            Assert.Equal(eventString, sut.BodyAsString);
        }

        [Fact]
        public void DoesNotThrowIfMessageFromStringAndOnlyMessageTypeSpecified()
        {
            var eventString = Guid.NewGuid().ToString();
            var sut = new Message<IEvent>(eventString, null)
            {
                MessageProperties = new()
                {
                    { _defaultMessageTypePropertyName, "StringMessage" }
                }
            };

            sut.Build(new());

            Assert.Equal(eventString, sut.BodyAsString);
        }

        [Theory]
        [InlineData("MyMessageType")]
        [InlineData("MyMessageType123")]
        public void DoesNotThrowIfMessageFromStringAndOnlyCustomMessageTypeSpecified(string messageTypePropertyName)
        {
            var eventString = Guid.NewGuid().ToString();
            var sut = new Message<IEvent>(eventString, null)
            {
                MessageProperties = new()
                {
                    { messageTypePropertyName, "StringMessage" }
                }
            };

            sut.Build(new() { MessageTypePropertyName = messageTypePropertyName });

            Assert.Equal(eventString, sut.BodyAsString);
        }
    }
}
