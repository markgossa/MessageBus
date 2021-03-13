using MessageBus.Abstractions.Tests.Unit.Handlers;
using MessageBus.Abstractions.Tests.Unit.Models.Commands;
using MessageBus.Abstractions.Tests.Unit.Models.Events;
using MessageBus.Abstractions.Tests.Unit.Services;
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
            _mockMessageProcessorResolver.Verify(m => m.Initialize(), Times.Once);
            _mockMessageBusAdminClient.Verify(m => m.ConfigureAsync(_messageSubscriptions, It.IsAny<MessageBusOptions>()), Times.Once);
        }

        [Fact]
        public async Task ConfiguresMessageBusWithOptionsAsync()
        {
            const string messageTypePropertyName = "MyMessageType";
            var sut = new MessageBus(_mockMessageHandlerResolver.Object, _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object,
                _mockMessageProcessorResolver.Object, new MessageBusOptions { MessageTypePropertyName = messageTypePropertyName });
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
        public async Task CallsMessageProcessorsAndMessageHandlersInOrder()
        {
            var order = 0;
            var mockMessagePreProcessor1 = new Mock<IMessagePreProcessor>();
            mockMessagePreProcessor1.Setup(m => m.ProcessAsync(It.IsAny<IMessageContext<AircraftTakenOff>>()))
                .Callback(() => Assert.Equal(1, ++order));
            var mockMessagePreProcessor2 = new Mock<IMessagePreProcessor>();
            mockMessagePreProcessor2.Setup(m => m.ProcessAsync(It.IsAny<IMessageContext<AircraftTakenOff>>()))
                .Callback(() => Assert.Equal(2, ++order));
            var mockMessageHandler = new Mock<IMessageHandler<AircraftTakenOff>>();
            _mockMessageHandlerResolver.Setup(m => m.Resolve(nameof(AircraftTakenOff))).Returns(mockMessageHandler.Object);
            mockMessageHandler.Setup(m => m.HandleAsync(It.IsAny<IMessageContext<AircraftTakenOff>>()))
                .Callback(() => Assert.Equal(3, ++order));
            var mockMessagePostProcessor1 = new Mock<IMessagePostProcessor>();
            mockMessagePostProcessor1.Setup(m => m.ProcessAsync(It.IsAny<IMessageContext<AircraftTakenOff>>()))
                .Callback(() => Assert.Equal(4, ++order));
            var mockMessagePostProcessor2 = new Mock<IMessagePostProcessor>();
            mockMessagePostProcessor2.Setup(m => m.ProcessAsync(It.IsAny<IMessageContext<AircraftTakenOff>>()))
            .Callback(() => Assert.Equal(5, ++order));
            _mockMessageProcessorResolver.Setup(m => m.GetMessagePreProcessors())
                .Returns(new List<IMessagePreProcessor> { mockMessagePreProcessor1.Object, mockMessagePreProcessor2.Object });
            _mockMessageProcessorResolver.Setup(m => m.GetMessagePostProcessors())
                .Returns(new List<IMessagePostProcessor> { mockMessagePostProcessor1.Object, mockMessagePostProcessor2.Object });
            var args = new MessageReceivedEventArgs(BuildAircraftTakenOffMessage(Guid.NewGuid().ToString()),
                new object(), new Dictionary<string, string> { { "MessageType", nameof(AircraftTakenOff) } });

            await _sut.OnMessageReceived(args);

            mockMessagePreProcessor1.Verify(m => m.ProcessAsync(It.Is<IMessageContext<AircraftTakenOff>>(c =>
                c.MessageId == args.MessageId)), Times.Once);
            mockMessagePreProcessor2.Verify(m => m.ProcessAsync(It.Is<IMessageContext<AircraftTakenOff>>(c =>
                c.MessageId == args.MessageId)), Times.Once);
            mockMessagePostProcessor1.Verify(m => m.ProcessAsync(It.Is<IMessageContext<AircraftTakenOff>>(c =>
                c.MessageId == args.MessageId)), Times.Once);
            mockMessagePostProcessor2.Verify(m => m.ProcessAsync(It.Is<IMessageContext<AircraftTakenOff>>(c =>
                c.MessageId == args.MessageId)), Times.Once);
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
                _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object, _mockMessageProcessorResolver.Object,
                new MessageBusOptions { MessageTypePropertyName = "MessageTypeIdentifier" });

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
                _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object, _mockMessageProcessorResolver.Object);

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

            _mockMessageHandlerResolver.Verify(m => m.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(nameof(AircraftLanded), null), 
                Times.Once);
        }

        [Theory]
        [InlineData("AircraftTakenOff")]
        [InlineData("AircraftLanded")]
        public void SubscribeToMessageUsesLabelIfNoMessageTypeSpecified(string label)
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                Label = label
            };

            _sut.SubscribeToMessage<AircraftLanded, AircraftLandedHandler>(subscriptionFilter);

            _mockMessageHandlerResolver.Verify(m => m.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(label,
                subscriptionFilter), Times.Once);
        }

        [Theory]
        [InlineData("AircraftTakenOff")]
        [InlineData("AircraftLanded")]
        public void SubscribeToMessageUsesMessageTypeIfLabelNull(string messageType)
        {
            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", messageType }
                }
            };

            _sut.SubscribeToMessage<AircraftLanded, AircraftLandedHandler>(subscriptionFilter);

            _mockMessageHandlerResolver.Verify(m => m.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(messageType,
                subscriptionFilter), Times.Once);
        }

        [Fact]
        public void SubscribeToMessageUsesLabelIfBothLabelAndMessageTypeSpecified()
        {
            const string expectedMessageType = "AircraftTakenOff";
            var subscriptionFilter = new SubscriptionFilter
            {
                Label = expectedMessageType,
                MessageProperties = new Dictionary<string, string>
                {
                    { "MessageType", "AircraftLanded" }
                }
            };

            _sut.SubscribeToMessage<AircraftLanded, AircraftLandedHandler>(subscriptionFilter);

            _mockMessageHandlerResolver.Verify(m => m.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(expectedMessageType,
                subscriptionFilter), Times.Once);
        }

        [Theory]
        [InlineData("AircraftTakenOff")]
        [InlineData("AircraftLanded")]
        public void SubscribeToMessageUsesCustomMessageTypeIfLabelNull(string messageType)
        {
            const string customMessageTypeProperty = "MyCustomMessageType";
            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>
                {
                    { customMessageTypeProperty, messageType }
                }
            };

            var sut = new MessageBus(_mockMessageHandlerResolver.Object, _mockMessageBusAdminClient.Object, _mockMessageBusClient.Object,
                _mockMessageProcessorResolver.Object, new MessageBusOptions { MessageTypePropertyName = customMessageTypeProperty });

            sut.SubscribeToMessage<AircraftLanded, AircraftLandedHandler>(subscriptionFilter);

            _mockMessageHandlerResolver.Verify(m => m.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(messageType,
                subscriptionFilter), Times.Once);
        }

        [Fact]
        public void SubscribesToMessagesWithCustomProperties()
        {
            const string expectedMessageType = "AL";
            var subscriptionFilter = new SubscriptionFilter
            {
                MessageProperties = new Dictionary<string, string>
                    {
                        { "MessageType", expectedMessageType }
                    }
            };

            _sut.SubscribeToMessage<AircraftLanded, AircraftLandedHandler>(subscriptionFilter);

            _mockMessageHandlerResolver.Verify(m => m.SubcribeToMessage<AircraftLanded, AircraftLandedHandler>(expectedMessageType,
                subscriptionFilter), Times.Once);
        }

        [Fact]
        public async Task PublishAsyncCallsMessageBusClient()
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
                _mockMessageBusClient.Object, _mockMessageProcessorResolver.Object, options);
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
                _mockMessageBusClient.Object, _mockMessageProcessorResolver.Object, options);
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

        [Fact]
        public async Task SendAsyncCallsMessageBusClient()
        {
            var aircraftId = Guid.NewGuid().ToString();
            var createNewFlightPlanCommand = new CreateNewFlightPlan { Destination = Guid.NewGuid().ToString() };
            var command = new Message<ICommand>(createNewFlightPlanCommand);

            await _sut.SendAsync(command);

            _mockMessageBusClient.Verify(m => m.SendAsync(command), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("MyMessageType")]
        [InlineData("MyMessageType2")]
        public async Task SendsCommandWithMessageTypeOnly(string messageTypePropertyName)
        {
            var createNewFlightPlan = new CreateNewFlightPlan { Destination = Guid.NewGuid().ToString() };
            var command = new Message<ICommand>(createNewFlightPlan);

            Message<ICommand> callbackCommand = null;
            _mockMessageBusClient.Setup(m => m.SendAsync(command)).Callback<Message<ICommand>>(a => callbackCommand = a);

            var options = new MessageBusOptions();
            if (messageTypePropertyName is not null)
            {
                options.MessageTypePropertyName = messageTypePropertyName;
            }

            var sut = new MessageBus(_mockMessageHandlerResolver.Object, _mockMessageBusAdminClient.Object,
                _mockMessageBusClient.Object, _mockMessageProcessorResolver.Object, options);
            await sut.SendAsync(command);

            Assert.Equal(nameof(CreateNewFlightPlan), callbackCommand.MessageProperties[messageTypePropertyName ?? "MessageType"]);
            Assert.False(callbackCommand.MessageProperties.ContainsKey("MessageVersion"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("MyMessageVersion")]
        [InlineData("MyMessageVersion2")]
        public async Task SendsCommandWithMessageTypeAndMessageVersion(string messageVersionPropertyName)
        {
            var createNewFlightPlan = new Models.Commands.V2.CreateNewFlightPlan { Destination = Guid.NewGuid().ToString() };
            var command = new Message<ICommand>(createNewFlightPlan);

            Message<ICommand> callbackCommand = null;
            _mockMessageBusClient.Setup(m => m.SendAsync(command)).Callback<Message<ICommand>>(a => callbackCommand = a);

            var options = new MessageBusOptions();
            if (messageVersionPropertyName is not null)
            {
                options.MessageVersionPropertyName = messageVersionPropertyName;
            }

            var sut = new MessageBus(_mockMessageHandlerResolver.Object, _mockMessageBusAdminClient.Object,
                _mockMessageBusClient.Object, _mockMessageProcessorResolver.Object, options);
            await sut.SendAsync(command);

            Assert.Equal(nameof(Models.Commands.V2.CreateNewFlightPlan), callbackCommand.MessageProperties["MessageType"]);
            Assert.Equal("2", callbackCommand.MessageProperties[messageVersionPropertyName ?? "MessageVersion"]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SendsCommandWithCustomMessageProperties(bool overrideDefaultProperties)
        {
            var createNewFlightPlan = new Models.Commands.V2.CreateNewFlightPlan { Destination = Guid.NewGuid().ToString() };
            var command = new Message<ICommand>(createNewFlightPlan)
            {
                OverrideDefaultMessageProperties = overrideDefaultProperties,
                MessageProperties = new Dictionary<string, string>
            {
                { "AircraftType", "Commercial" },
                { "AircraftSize", "Heavy" }
            }
            };

            Message<ICommand> callbackCommand = null;
            _mockMessageBusClient.Setup(m => m.SendAsync(command)).Callback<Message<ICommand>>(a => callbackCommand = a);

            await _sut.SendAsync(command);

            Assert.Equal("Commercial", callbackCommand.MessageProperties["AircraftType"]);
            Assert.Equal("Heavy", callbackCommand.MessageProperties["AircraftSize"]);
            Assert.Equal(overrideDefaultProperties, !callbackCommand.MessageProperties.ContainsKey("MessageType"));
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", false)]
        [InlineData("My message", true)]
        [InlineData("Hello world!", false)]
        public async Task SendsCommandAsStringWithCustomMessageProperties(string messageString, bool overrideDefaultProperties)
        {
            var command = new Message<ICommand>(messageString)
            {
                OverrideDefaultMessageProperties = overrideDefaultProperties,
                MessageProperties = new Dictionary<string, string>
            {
                { "AircraftType", "Commercial" },
                { "AircraftSize", "Heavy" }
            }
            };

            Message<ICommand> callbackEvent = null;
            _mockMessageBusClient.Setup(m => m.SendAsync(command)).Callback<Message<ICommand>>(a => callbackEvent = a);

            await _sut.SendAsync(command);

            Assert.Equal(messageString, callbackEvent.BodyAsString);
            Assert.Equal("Commercial", callbackEvent.MessageProperties["AircraftType"]);
            Assert.Equal("Heavy", callbackEvent.MessageProperties["AircraftSize"]);
            Assert.False(callbackEvent.MessageProperties.ContainsKey("MessageType"));
            Assert.False(callbackEvent.MessageProperties.ContainsKey("MessageVersion"));
        }

        [Fact]
        public void AddsMessageProcessors()
        {
            _sut.AddMessagePreProcessor<TestPreProcessor1>();
            _sut.AddMessagePostProcessor<TestPostProcessor1>();

            _mockMessageProcessorResolver.Verify(m => m.AddMessagePreProcessor<TestPreProcessor1>(), Times.Once);
            _mockMessageProcessorResolver.Verify(m => m.AddMessagePostProcessor<TestPostProcessor1>(), Times.Once);
        }

        [Fact]
        public async Task SendsMessageCopyWithoutDelay()
        {
            var messageObject = new object();
            await _sut.SendMessageCopyAsync(messageObject);

            _mockMessageBusClient.Verify(m => m.SendMessageCopyAsync(messageObject, 0), Times.Once);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        public async Task SendsMessageCopyWithDelayInSeconds(int delayInSeconds)
        {
            var messageObject = new object();
            await _sut.SendMessageCopyAsync(messageObject, delayInSeconds);

            _mockMessageBusClient.Verify(m => m.SendMessageCopyAsync(messageObject, delayInSeconds), Times.Once);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        public async Task SendsMessageCopyWithEnqueueTime(int delayInSeconds)
        {
            var messageObject = new object();
            var enqueueTime = DateTimeOffset.Now.AddSeconds(delayInSeconds);
            await _sut.SendMessageCopyAsync(messageObject, enqueueTime);

            _mockMessageBusClient.Verify(m => m.SendMessageCopyAsync(messageObject, enqueueTime), Times.Once);
        }
    }
}
