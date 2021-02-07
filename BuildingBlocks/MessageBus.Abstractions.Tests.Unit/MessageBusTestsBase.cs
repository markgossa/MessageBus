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
        protected readonly Mock<IMessageBusHandlerResolver> _mockMessageBusHandlerResolver = new Mock<IMessageBusHandlerResolver>();
        protected readonly List<MessageSubscription> _messasgeSubscriptions = new List<MessageSubscription>
        {
            new MessageSubscription(typeof(AircraftLandedHandler), typeof(AircraftTakenOffHandler))
        };

        protected readonly Mock<IMessageBusAdminClient> _mockMessageBusAdminClient = new Mock<IMessageBusAdminClient>();
        protected readonly Mock<IMessageBusClient> _mockMessageBusClient = new Mock<IMessageBusClient>();
        protected MessageBus _sut;

        protected MessageBusTestsBase()
        {
            _mockMessageBusHandlerResolver.Setup(m => m.GetMessageSubscriptions()).Returns(_messasgeSubscriptions);
            _sut = new MessageBus(_mockMessageBusHandlerResolver.Object, _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object);
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