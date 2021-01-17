using MessageBus.Abstractions.Tests.Unit.Handlers;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageBusServiceTests
    {
        [Fact]
        public async Task ConfiguresMessageBusAsync()
        {
            var mockMessageBusHandlerResolver = new Mock<IMessageBusHandlerResolver>();
            var handlers = new List<Type> { typeof(AircraftLandedHandler), typeof(AircraftTakenOffHandler) };
            mockMessageBusHandlerResolver.Setup(m => m.GetMessageHandlers()).Returns(handlers);
            var mockMessageBusAdmin = new Mock<IMessageBusAdminClient>();
            var mockMessageBusClient = new Mock<IMessageBusClient>();
            var sut = new MessageBusService(mockMessageBusHandlerResolver.Object,
                mockMessageBusAdmin.Object, mockMessageBusClient.Object);

            await sut.ConfigureAsync();

            mockMessageBusAdmin.Verify(m => m.ConfigureAsync(handlers), Times.Once);
        }
        
        [Fact]
        public async Task CallsCorrectMessageHandler()
        {
            var mockMessageBusHandlerResolver = new Mock<IMessageBusHandlerResolver>();
            var handlers = new List<Type> { typeof(AircraftLandedHandler), typeof(AircraftTakenOffHandler) };
            mockMessageBusHandlerResolver.Setup(m => m.GetMessageHandlers()).Returns(handlers);
            var mockMessageBusAdmin = new Mock<IMessageBusAdminClient>();
            var mockMessageBusClient = new Mock<IMessageBusClient>();
            var sut = new MessageBusService(mockMessageBusHandlerResolver.Object,
                mockMessageBusAdmin.Object, mockMessageBusClient.Object);

            await sut.StartAsync();
        }
    }
}
