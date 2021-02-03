using MessageBus.Abstractions.Tests.Unit.Handlers;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageBusReceiverTests : MessageBusReceiverTestsBase
    {
        [Fact]
        public async Task ConfiguresMessageBusAsync()
        {
            await _sut.ConfigureAsync();

            _mockMessageBusAdminClient.Verify(m => m.ConfigureAsync(_handlers), Times.Once);
        }
        
        [Fact]
        public async Task StartsMessageBusClient()
        {
            await _sut.StartAsync();

            _mockMessageBusClient.Verify(m => m.StartAsync(), Times.Once);
            _mockMessageBusClient.Verify(m => m.AddMessageHandler(It.IsAny<Func<MessageReceivedEventArgs, Task>>()), Times.Once);
            _mockMessageBusClient.Verify(m => m.AddErrorMessageHandler(It.IsAny<Func<MessageErrorReceivedEventArgs, Task>>()), Times.Once);
        }

        [Fact]
        public async Task CallsCorrectMessageHandler1()
        {
            var mockAircraftTakenOffHandler = new AircraftTakenOffHandler();
            _mockMessageBusHandlerResolver.Setup(m => m.Resolve(nameof(AircraftTakenOff)))
                .Returns(mockAircraftTakenOffHandler);
            var aircraftId = Guid.NewGuid().ToString();
            var args = new MessageReceivedEventArgs(BuildAircraftTakenOffMessage(aircraftId),
                new object(), new Dictionary<string, string> { { "MessageType", nameof(AircraftTakenOff) } });

            await _sut.OnMessageReceived(args);

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

            var aircraftId = Guid.NewGuid().ToString();
            var args = new MessageReceivedEventArgs(BuildAircraftLandedMessage(aircraftId),
                new object(), new Dictionary<string, string> { { "MessageType", nameof(AircraftLanded) } });

            await _sut.OnMessageReceived(args);

            _mockMessageBusHandlerResolver.Verify(m => m.Resolve(nameof(AircraftLanded)), Times.Once);
            Assert.Equal(aircraftId, mockAircraftTakenOffHandler.AircraftId);
            Assert.Equal(1, mockAircraftTakenOffHandler.MessageCount);
        }
        
        [Fact]
        public async Task MessageContextPropertiesAvailableToMessageHandler()
        {
            var mockAircraftTakenOffHandler = new AircraftLandedHandler();
            _mockMessageBusHandlerResolver.Setup(m => m.Resolve(nameof(AircraftLanded)))
                .Returns(mockAircraftTakenOffHandler);
            var aircraftId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var correlationId = Guid.NewGuid().ToString();
            var messageProperties = new Dictionary<string, string> 
            { 
                { "MessageType", nameof(AircraftLanded) },
                { "MessageVersion", "1" }
            };

            var args = new MessageReceivedEventArgs(BuildAircraftLandedMessage(aircraftId),
                new object(), messageProperties)
            {
                MessageId = messageId,
                CorrelationId = correlationId,
                DeliveryCount = 2
            };

            await _sut.OnMessageReceived(args);

            _mockMessageBusHandlerResolver.Verify(m => m.Resolve(nameof(AircraftLanded)), Times.Once);
            Assert.Equal(aircraftId, mockAircraftTakenOffHandler.MessageContext.Message.AircraftId);
            Assert.Equal(1, mockAircraftTakenOffHandler.MessageCount);
            Assert.Equal(messageId, mockAircraftTakenOffHandler.MessageContext.MessageId);
            Assert.Equal(correlationId, mockAircraftTakenOffHandler.MessageContext.CorrelationId);
            Assert.Equal(nameof(AircraftLanded), mockAircraftTakenOffHandler.MessageContext.Properties["MessageType"]);
            Assert.Equal(2, mockAircraftTakenOffHandler.MessageContext.DeliveryCount);
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
            var args = new MessageReceivedEventArgs(BuildAircraftLandedMessage(aircraftId),
                new object(), new Dictionary<string, string> { { "MessageTypeIdentifier", nameof(AircraftLanded) } });

            await sut.OnMessageReceived(args);

            _mockMessageBusHandlerResolver.Verify(m => m.Resolve(nameof(AircraftLanded)), Times.Once);
            Assert.Equal(aircraftId, mockAircraftTakenOffHandler.AircraftId);
            Assert.Equal(1, mockAircraftTakenOffHandler.MessageCount);
        }

        [Fact]
        public async Task ThrowsCorrectExcepetionWhenErrorProcessingMessage()
        {
            const string errorMessage = "Unable to process message";
            var errorMessageEventArgs = new MessageErrorReceivedEventArgs(new ApplicationException(
                errorMessage));

            var exception = await Assert.ThrowsAsync<MessageReceivedException>(async () 
                => await _sut.OnErrorMessageReceived(errorMessageEventArgs));

            Assert.Equal(errorMessage, exception.InnerException.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid message")]
        public async Task DeadLetterAsyncCallsMessageBusClient(string reason)
        {
            var message = Guid.NewGuid();
            await _sut.DeadLetterMessageAsync(message, reason);

            _mockMessageBusClient.Verify(m => m.DeadLetterMessageAsync(message, reason), Times.Once);
        }
    }
}
