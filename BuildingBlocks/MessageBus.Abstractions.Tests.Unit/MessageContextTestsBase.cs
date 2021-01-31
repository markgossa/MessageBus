using MessageBus.Abstractions.Tests.Unit.Models.Events;
using Moq;
using System;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageContextTestsBase
    {
        protected const string _aircraftId = "3df0e37b-78cf-4ec5-973f-f56778e38be1";
        protected readonly string _messageAsString = $"{{\"AircraftId\": \"{_aircraftId}\"}}";
        protected readonly Mock<IMessageBusReceiver> _mockMessageBusReceiver = new Mock<IMessageBusReceiver>();
        protected MessageContext<AircraftLanded> _sut;
        protected object _messageObject = new object();

        public MessageContextTestsBase()
        {
            _sut = new MessageContext<AircraftLanded>(new BinaryData(_messageAsString), _messageObject, _mockMessageBusReceiver.Object);
        }
    }
}