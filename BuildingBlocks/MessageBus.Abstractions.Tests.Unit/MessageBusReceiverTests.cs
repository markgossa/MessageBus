using MessageBus.Abstractions.Tests.Unit.Handlers;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageBusReceiverTests : MessageBusReceiverTestsBase
    {
        [Fact]
        public async Task ConfiguresMessageBusAsync()
        {
            var sut = new MessageBusReceiver(_mockMessageBusHandlerResolver.Object,
                _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object);

            await sut.ConfigureAsync();

            _mockMessageBusAdminClient.Verify(m => m.ConfigureAsync(_handlers), Times.Once);
            _mockMessageBusClient.Verify(m => m.AddMessageHandler(It.IsAny<Func<MessageReceivedEventArgs, Task>>()), Times.Once);
        }
        
        [Fact]
        public async Task StartsMessageBusClient()
        {
            var sut = new MessageBusReceiver(_mockMessageBusHandlerResolver.Object,
                _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object);

            await sut.StartAsync();

            _mockMessageBusClient.Verify(m => m.StartAsync(), Times.Once);
        }
        
        [Fact]
        public async Task CallsCorrectMessageHandler1()
        {
            var mockAircraftTakenOffHandler = new AircraftTakenOffHandler();
            _mockMessageBusHandlerResolver.Setup(m => m.Resolve(nameof(AircraftTakenOff)))
                .Returns(mockAircraftTakenOffHandler);
            var sut = new MessageBusReceiver(_mockMessageBusHandlerResolver.Object,
                _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object);

            var aircraftId = Guid.NewGuid().ToString();
            var args = new MessageReceivedEventArgs(BuildAircraftTakenOffMessage(aircraftId));
            args.MessageProperties.Add("MessageType", nameof(AircraftTakenOff));

            await sut.OnMessageReceived(args);

            _mockMessageBusHandlerResolver.Verify(m => m.Resolve(nameof(AircraftTakenOff)), Times.Once);
            Assert.Equal(aircraftId, mockAircraftTakenOffHandler.AircraftId);
            Assert.Equal(1, mockAircraftTakenOffHandler.MessageCount);
        }
        
        [Fact]
        public async Task CallsCorrectMessageHandler2()
        {
            var mockAircraftTakenOffHandler = new AircraftLandedHandler();
            _mockMessageBusHandlerResolver.Setup(m => m.Resolve(nameof(AircraftLanded)))
                .Returns(mockAircraftTakenOffHandler);
            var sut = new MessageBusReceiver(_mockMessageBusHandlerResolver.Object,
                _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object);

            var aircraftId = Guid.NewGuid().ToString();
            var args = new MessageReceivedEventArgs(BuildAircraftLandedMessage(aircraftId));
            args.MessageProperties.Add("MessageType", nameof(AircraftLanded));

            await sut.OnMessageReceived(args);

            _mockMessageBusHandlerResolver.Verify(m => m.Resolve(nameof(AircraftLanded)), Times.Once);
            Assert.Equal(aircraftId, mockAircraftTakenOffHandler.AircraftId);
            Assert.Equal(1, mockAircraftTakenOffHandler.MessageCount);
        }

        [Fact]
        public async Task CallsCorrectMessageHandlerWithCustomMessageTypePropertyName()
        {
            var mockAircraftTakenOffHandler = new AircraftLandedHandler();
            _mockMessageBusHandlerResolver.Setup(m => m.Resolve(nameof(AircraftLanded)))
                .Returns(mockAircraftTakenOffHandler);
            var sut = new MessageBusReceiver(_mockMessageBusHandlerResolver.Object,
                _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object, new MessageBusSettings
                { MessageTypeProperty = "MessageTypeIdentifier" });

            var aircraftId = Guid.NewGuid().ToString();
            var args = new MessageReceivedEventArgs(BuildAircraftLandedMessage(aircraftId));
            args.MessageProperties.Add("MessageTypeIdentifier", nameof(AircraftLanded));

            await sut.OnMessageReceived(args);

            _mockMessageBusHandlerResolver.Verify(m => m.Resolve(nameof(AircraftLanded)), Times.Once);
            Assert.Equal(aircraftId, mockAircraftTakenOffHandler.AircraftId);
            Assert.Equal(1, mockAircraftTakenOffHandler.MessageCount);
        }

        [Fact]
        public async Task ThrowsCorrectExcepetionWhenErrorProcessingMessage()
        {
            var sut = new MessageBusReceiver(_mockMessageBusHandlerResolver.Object,
                _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object);

            const string errorMessage = "Unable to process message";
            var errorMessageEventArgs = new ErrorMessageReceivedEventArgs(new ApplicationException(
                errorMessage));

            var exception = await Assert.ThrowsAsync<MessageReceivedException>(async () 
                => await sut.OnErrorMessageReceived(errorMessageEventArgs));

            Assert.Equal(errorMessage, exception.InnerException.Message);
        }
    }
}
