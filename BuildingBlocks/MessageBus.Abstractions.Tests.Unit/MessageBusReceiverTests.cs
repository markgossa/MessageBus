using MessageBus.Abstractions.Tests.Unit.Handlers;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageBusReceiverTests
    {
        private readonly Mock<IMessageBusHandlerResolver> _mockMessageBusHandlerResolver = new Mock<IMessageBusHandlerResolver>();
        private readonly List<Type> _handlers = new List<Type> { typeof(AircraftLandedHandler), typeof(AircraftTakenOffHandler) };
        private readonly Mock<IMessageBusAdminClient> _mockMessageBusAdmin = new Mock<IMessageBusAdminClient>();
        private readonly Mock<IMessageBusClient> _mockMessageBusClient = new Mock<IMessageBusClient>();
        
        public MessageBusReceiverTests()
        {
            _mockMessageBusHandlerResolver.Setup(m => m.GetMessageHandlers()).Returns(_handlers);
        }

        [Fact]
        public async Task ConfiguresMessageBusAsync()
        {
            var sut = new MessageBusReceiver(_mockMessageBusHandlerResolver.Object,
                _mockMessageBusAdmin.Object, _mockMessageBusClient.Object);

            await sut.ConfigureAsync();

            _mockMessageBusAdmin.Verify(m => m.ConfigureAsync(_handlers), Times.Once);
        }
        
        [Fact]
        public async Task StartsMessageBusClient()
        {
            var sut = new MessageBusReceiver(_mockMessageBusHandlerResolver.Object,
                _mockMessageBusAdmin.Object, _mockMessageBusClient.Object);

            await sut.StartAsync();

            _mockMessageBusClient.Verify(m => m.StartAsync(), Times.Once);
        }
        
        [Fact]
        public async Task CallsCorrectMessageHandler()
        {
            var mockAircraftTakenOffHandler = new AircraftTakenOffHandler();
            _mockMessageBusHandlerResolver.Setup(m => m.Resolve(nameof(AircraftTakenOff)))
                .Returns(mockAircraftTakenOffHandler);
            var sut = new MessageBusReceiver(_mockMessageBusHandlerResolver.Object,
                _mockMessageBusAdmin.Object, _mockMessageBusClient.Object);

            var aircraftId = Guid.NewGuid().ToString();
            var message = JsonSerializer.Serialize(new AircraftTakenOff { AicraftId = aircraftId });

            //_mockMessageBusClient.Raise(m => m.AddMessageHandler += null, EventArgs.Empty);

            //await sut.HandleMessageAsync(message, nameof(AircraftTakenOff));

            _mockMessageBusHandlerResolver.Verify(m => m.Resolve(nameof(AircraftTakenOff)), Times.Once);
            Assert.Equal(aircraftId, mockAircraftTakenOffHandler.AircraftId);
            Assert.Equal(1, mockAircraftTakenOffHandler.MessageCount);
        }
    }
}
