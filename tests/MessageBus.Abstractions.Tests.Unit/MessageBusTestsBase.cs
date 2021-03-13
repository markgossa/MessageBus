using MessageBus.Abstractions.Tests.Unit.Handlers;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageBusTestsBase
    {
        protected const string _messageTypeDefaultPropertyName = "MessageType";
        protected readonly Mock<IMessageHandlerResolver> _mockMessageHandlerResolver = new Mock<IMessageHandlerResolver>();
        protected readonly List<MessageHandlerMapping> _messageSubscriptions;

        protected readonly Mock<IMessageBusAdminClient> _mockMessageBusAdminClient = new Mock<IMessageBusAdminClient>();
        protected readonly Mock<IMessageBusClient> _mockMessageBusClient = new Mock<IMessageBusClient>();
        protected readonly Mock<IMessageProcessorResolver> _mockMessageProcessorResolver = new Mock<IMessageProcessorResolver>();
        protected MessageBus _sut;

        protected MessageBusTestsBase()
        {
            _messageSubscriptions = BuildMessageSubscriptions();

            _mockMessageHandlerResolver.Setup(m => m.GetMessageSubscriptions()).Returns(_messageSubscriptions);
            _sut = new MessageBus(_mockMessageHandlerResolver.Object, _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object,
                _mockMessageProcessorResolver.Object);
        }

        private static List<MessageHandlerMapping> BuildMessageSubscriptions()
        {
            var subscriptionFilter = new SubscriptionFilter();
            subscriptionFilter.Build(_messageTypeDefaultPropertyName, typeof(AircraftLandedHandler));

            return new List<MessageHandlerMapping>
        {
            new MessageHandlerMapping(typeof(AircraftLandedHandler), typeof(AircraftTakenOffHandler), subscriptionFilter)
        };
        }

        protected static BinaryData BuildAircraftTakenOffMessage(string aircraftId)
        {
            var messageBody = JsonSerializer.Serialize(new AircraftTakenOff { AircraftId = aircraftId });
            return new BinaryData(Encoding.UTF8.GetBytes(messageBody));
        }

        protected static BinaryData BuildAircraftLandedMessage(string aircraftId)
        {
            var messageBody = JsonSerializer.Serialize(new AircraftLanded { AircraftId = aircraftId });
            return new BinaryData(Encoding.UTF8.GetBytes(messageBody));
        }
    }
}