using MessageBus.Abstractions.Tests.Unit.Handlers;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageBusTests : MessageBusTestsBase
    {
        [Fact]
        public async Task ConfiguresMessageBusAsync()
        {
            await _sut.ConfigureAsync();

            _mockMessageHandlerResolver.Verify(m => m.Initialize(), Times.Once);
            _mockMessageBusAdminClient.Verify(m => m.ConfigureAsync(_messageSubscriptions, It.IsAny<MessageBusOptions>()), Times.Once);
        }
        
        [Fact]
        public async Task ConfiguresMessageBusWithOptionsAsync()
        {
            const string messageTypePropertyName = "MyMessageType";
            var sut = new MessageBus(_mockMessageHandlerResolver.Object, _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object,
                new MessageBusOptions { MessageTypePropertyName = messageTypePropertyName });
            await sut.ConfigureAsync();

            _mockMessageHandlerResolver.Verify(m => m.Initialize(), Times.Once);
            _mockMessageBusAdminClient.Verify(m => m.ConfigureAsync(_messageSubscriptions, It.Is<MessageBusOptions>(m => 
                m.MessageTypePropertyName == messageTypePropertyName)), Times.Once);
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
            _mockMessageHandlerResolver.Setup(m => m.Resolve(nameof(AircraftTakenOff)))
                .Returns(mockAircraftTakenOffHandler);
            var aircraftId = Guid.NewGuid().ToString();
            var args = new MessageReceivedEventArgs(BuildAircraftTakenOffMessage(aircraftId),
                new object(), new Dictionary<string, string> { { "MessageType", nameof(AircraftTakenOff) } });

            await _sut.OnMessageReceived(args);

            _mockMessageHandlerResolver.Verify(m => m.Resolve(nameof(AircraftTakenOff)), Times.Once);
            Assert.Equal(aircraftId, mockAircraftTakenOffHandler.AircraftId);
            Assert.Equal(1, mockAircraftTakenOffHandler.MessageCount);
        }
        
        [Fact]
        public async Task CallsCorrectMessageHandler2()
        {
            var mockAircraftTakenOffHandler = new AircraftLandedHandler();
            _mockMessageHandlerResolver.Setup(m => m.Resolve(nameof(AircraftLanded)))
                .Returns(mockAircraftTakenOffHandler);

            var aircraftId = Guid.NewGuid().ToString();
            var args = new MessageReceivedEventArgs(BuildAircraftLandedMessage(aircraftId),
                new object(), new Dictionary<string, string> { { "MessageType", nameof(AircraftLanded) } });

            await _sut.OnMessageReceived(args);

            _mockMessageHandlerResolver.Verify(m => m.Resolve(nameof(AircraftLanded)), Times.Once);
            Assert.Equal(aircraftId, mockAircraftTakenOffHandler.AircraftId);
            Assert.Equal(1, mockAircraftTakenOffHandler.MessageCount);
        }
        
        [Fact]
        public async Task MessageContextPropertiesAvailableToMessageHandler()
        {
            var mockAircraftTakenOffHandler = new AircraftLandedHandler();
            _mockMessageHandlerResolver.Setup(m => m.Resolve(nameof(AircraftLanded)))
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

            _mockMessageHandlerResolver.Verify(m => m.Resolve(nameof(AircraftLanded)), Times.Once);
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
            _mockMessageHandlerResolver.Setup(m => m.Resolve(nameof(AircraftLanded)))
                .Returns(mockAircraftTakenOffHandler);
            var sut = new MessageBus(_mockMessageHandlerResolver.Object,
                _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object, new MessageBusOptions
                { MessageTypePropertyName = "MessageTypeIdentifier" });

            var aircraftId = Guid.NewGuid().ToString();
            var args = new MessageReceivedEventArgs(BuildAircraftLandedMessage(aircraftId),
                new object(), new Dictionary<string, string> { { "MessageTypeIdentifier", nameof(AircraftLanded) } });

            await sut.OnMessageReceived(args);

            _mockMessageHandlerResolver.Verify(m => m.Resolve(nameof(AircraftLanded)), Times.Once);
            Assert.Equal(aircraftId, mockAircraftTakenOffHandler.AircraftId);
            Assert.Equal(1, mockAircraftTakenOffHandler.MessageCount);
        }
        
        [Fact]
        public async Task ThrowsIfMessageHandlerNotFound()
        {
            var sut = new MessageBus(_mockMessageHandlerResolver.Object,
                _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object);

            var aircraftId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var args = new MessageReceivedEventArgs(BuildAircraftLandedMessage(aircraftId),
                new object(), new Dictionary<string, string> { { "MessageType", nameof(AircraftLanded) } })
            {
                MessageId = messageId
            };

            var ex = await Assert.ThrowsAsync<MessageHandlerNotFoundException>(async () => await sut.OnMessageReceived(args));

            Assert.Contains(messageId, ex.Message);
            _mockMessageHandlerResolver.Verify(m => m.Resolve(nameof(AircraftLanded)), Times.Once);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CheckHealthAsyncCallsServiceBusAdminClient(bool healthCheckResponse)
        {
            _mockMessageBusAdminClient.Setup(m => m.CheckHealthAsync()).ReturnsAsync(healthCheckResponse);

            var isHealthy = await _sut.CheckHealthAsync();

            _mockMessageBusAdminClient.Verify(m => m.CheckHealthAsync(), Times.Once);
            Assert.Equal(healthCheckResponse, isHealthy);
        }

        [Fact]
        public async Task StopAsyncStopsMessageBusClient()
        {
            await _sut.StopAsync();
            
            _mockMessageBusClient.Verify(m => m.StopAsync(), Times.Once);
        }
        
        [Fact]
        public void SubscribesToMessages()
        {
            _sut.SubscribeToMessage<AircraftLanded, AircraftLandedHandler>();

            _mockMessageHandlerResolver.Verify(m => m.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(null), Times.Once);
        }
        
        [Fact]
        public void SubscribesToMessagesWithCustomProperties()
        {
            var messageProperties = new Dictionary<string, string>();
            _sut.SubscribeToMessage<AircraftLanded, AircraftLandedHandler>(messageProperties);

            _mockMessageHandlerResolver.Verify(m => m.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(messageProperties), 
                Times.Once);
        }

        [Fact]
        public async Task PublishMessageAsyncCallsMessageBusClient()
        {
            var aircraftId = Guid.NewGuid().ToString();
            var aircraftLandedEvent = new AircraftLanded { AircraftId = aircraftId };
            var eventToSend = new Message<IEvent>(aircraftLandedEvent);
            
            await _sut.PublishAsync(eventToSend);

            _mockMessageBusClient.Verify(m => m.PublishAsync(eventToSend), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("MyMessageType")]
        [InlineData("MyMessageType2")]
        public async Task PublishesEventWithMessageTypeOnly(string messageTypePropertyName)
        {
            var aircraftlandedEvent = new AircraftLanded { AircraftId = Guid.NewGuid().ToString() };
            var eventObject = new Message<IEvent>(aircraftlandedEvent);
            
            Message<IEvent> callbackEvent = null;
            _mockMessageBusClient.Setup(m => m.PublishAsync(eventObject)).Callback<Message<IEvent>>(a => callbackEvent = a);
            
            var options = new MessageBusOptions();
            if (messageTypePropertyName is not null)
            {
                options.MessageTypePropertyName = messageTypePropertyName;
            }

            var sut = new MessageBus(_mockMessageHandlerResolver.Object, _mockMessageBusAdminClient.Object, 
                _mockMessageBusClient.Object, options);
            await sut.PublishAsync(eventObject);

            Assert.Equal(nameof(AircraftLanded), callbackEvent.MessageProperties[messageTypePropertyName ?? "MessageType"]);
            Assert.False(callbackEvent.MessageProperties.ContainsKey("MessageVersion"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("MyMessageVersion")]
        [InlineData("MyMessageVersion2")]
        public async Task PublishesEventWithMessageTypeAndMessageVersion(string messageVersionPropertyName)
        {
            var aircraftlandedEvent = new Models.Events.V2.AircraftLanded { AircraftId = Guid.NewGuid().ToString() };
            var eventObject = new Message<IEvent>(aircraftlandedEvent);

            Message<IEvent> callbackEvent = null;
            _mockMessageBusClient.Setup(m => m.PublishAsync(eventObject)).Callback<Message<IEvent>>(a => callbackEvent = a);

            var options = new MessageBusOptions();
            if (messageVersionPropertyName is not null)
            {
                options.MessageVersionPropertyName = messageVersionPropertyName;
            }

            var sut = new MessageBus(_mockMessageHandlerResolver.Object, _mockMessageBusAdminClient.Object,
                _mockMessageBusClient.Object, options);
            await sut.PublishAsync(eventObject);

            Assert.Equal(nameof(Models.Events.V2.AircraftLanded), callbackEvent.MessageProperties["MessageType"]);
            Assert.Equal("2", callbackEvent.MessageProperties[messageVersionPropertyName ?? "MessageVersion"]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PublishesEventWithCustomMessageProperties(bool overrideDefaultProperties)
        {
            var aircraftlandedEvent = new AircraftLanded { AircraftId = Guid.NewGuid().ToString() };
            var eventObject = new Message<IEvent>(aircraftlandedEvent)
            {
                OverrideDefaultMessageProperties = overrideDefaultProperties,
                MessageProperties = new Dictionary<string, string>
            {
                { "AircraftType", "Commercial" },
                { "AircraftSize", "Heavy" }
            }
            };

            Message<IEvent> callbackEvent = null;
            _mockMessageBusClient.Setup(m => m.PublishAsync(eventObject)).Callback<Message<IEvent>>(a => callbackEvent = a);

            await _sut.PublishAsync(eventObject);

            Assert.Equal("Commercial", callbackEvent.MessageProperties["AircraftType"]);
            Assert.Equal("Heavy", callbackEvent.MessageProperties["AircraftSize"]);
            Assert.Equal(overrideDefaultProperties, !callbackEvent.MessageProperties.ContainsKey("MessageType"));
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", false)]
        [InlineData("My message", true)]
        [InlineData("Hello world!", false)]
        public async Task PublishesEventAsStringWithCustomMessageProperties(string messageString, bool overrideDefaultProperties)
        {
            var eventObject = new Message<IEvent>(messageString)
            {
                OverrideDefaultMessageProperties = overrideDefaultProperties,
                MessageProperties = new Dictionary<string, string>
            {
                { "AircraftType", "Commercial" },
                { "AircraftSize", "Heavy" }
            }
            };

            Message<IEvent> callbackEvent = null;
            _mockMessageBusClient.Setup(m => m.PublishAsync(eventObject)).Callback<Message<IEvent>>(a => callbackEvent = a);

            await _sut.PublishAsync(eventObject);

            Assert.Equal(messageString, callbackEvent.BodyAsString);
            Assert.Equal("Commercial", callbackEvent.MessageProperties["AircraftType"]);
            Assert.Equal("Heavy", callbackEvent.MessageProperties["AircraftSize"]);
            Assert.False(callbackEvent.MessageProperties.ContainsKey("MessageType"));
            Assert.False(callbackEvent.MessageProperties.ContainsKey("MessageVersion"));
        }

    }
}
