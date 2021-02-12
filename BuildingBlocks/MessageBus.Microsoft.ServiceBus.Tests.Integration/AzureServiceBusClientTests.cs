using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class AzureServiceBusClientTests : MessageBusTestsBase
    {
        private readonly Mock<ITestHandler> mockTestHandler = new Mock<ITestHandler>();

        [Fact]
        public async Task CallsCorrectMessageHandlerUsingConnectionString()
        {
            var mockTestHandler = new Mock<ITestHandler>();
            var subscription = nameof(CallsCorrectMessageHandlerUsingConnectionString);
            var aircraftTakenOffEvent = await CreateSubscriptionAndSendAircraftTakenOffEvent(subscription);

            var sut = new AzureServiceBusClient(_connectionString, _topic, subscription);
            AddHandlers(mockTestHandler, sut);
            await sut.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(5));
            mockTestHandler.Verify(m => m.MessageHandler(It.Is<MessageReceivedEventArgs>(m =>
                GetAircraftIdFromMessage(m.Message) == aircraftTakenOffEvent.AircraftId)),
                Times.Once);
        }

        [Fact]
        public async Task CallsCorrectMessageHandlerUsingManagedIdentity()
        {
            var mockTestHandler = new Mock<ITestHandler>();
            var subscription = nameof(CallsCorrectMessageHandlerUsingManagedIdentity);
            var aircraftlandedEvent = await CreateSubscriptionAndSendAircraftLandedEvent(subscription);

            var sut = new AzureServiceBusClient(_hostname, _topic, subscription, _tenantId);
            AddHandlers(mockTestHandler, sut);
            await sut.StartAsync();
            
            await Task.Delay(TimeSpan.FromSeconds(5));
            mockTestHandler.Verify(m => m.MessageHandler(It.Is<MessageReceivedEventArgs>(m =>
                GetAircraftIdFromMessage(m.Message) == aircraftlandedEvent.AircraftId)),
                Times.Once);
        }

        [Fact]
        public async Task DeadLettersMessageWithoutReasonAsync()
        {
            var subscription = nameof(DeadLettersMessageWithoutReasonAsync);
            var aircraftlandedEvent = await CreateSubscriptionAndSendAircraftLandedEvent(subscription);
            var aircraftLandedHandler = new AircraftLandedHandler();
            var mockMessageHandlerResolver = new Mock<IMessageHandlerResolver>();
            mockMessageHandlerResolver.Setup(m => m.Resolve(nameof(AircraftLanded))).Returns(aircraftLandedHandler);

            var sut = new AzureServiceBusClient(_hostname, _topic, nameof(DeadLettersMessageWithoutReasonAsync), _tenantId);
            var messageBus = new Abstractions.MessageBus(mockMessageHandlerResolver.Object, new Mock<IMessageBusAdminClient>().Object,
                sut);

            await messageBus.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(2));
            var messages = await ReceiveMessagesForSubscriptionAsync(subscription, true);

            Assert.Equal(1, aircraftLandedHandler.MessageCount);
            Assert.Single(messages.Where(m => IsMatchingAircraftId<AircraftLanded>(m, aircraftlandedEvent.AircraftId)));
        }
        
        [Fact]
        public async Task DeadLettersMessageWithReasonAsync()
        {
            const string deadLetterReason = "Json Serliazation issue";
            var subscription = nameof(DeadLettersMessageWithReasonAsync);
            var aircraftlandedEvent = await CreateSubscriptionAndSendAircraftLandedEvent(subscription);
            var aircraftLandedHandler = new AircraftLandedHandler(deadLetterReason);
            var mockMessageHandlerResolver = new Mock<IMessageHandlerResolver>();
            mockMessageHandlerResolver.Setup(m => m.Resolve(nameof(AircraftLanded))).Returns(aircraftLandedHandler);

            var sut = new AzureServiceBusClient(_hostname, _topic, nameof(DeadLettersMessageWithReasonAsync), _tenantId);
            var messageBus = new Abstractions.MessageBus(mockMessageHandlerResolver.Object, new Mock<IMessageBusAdminClient>().Object,
                sut);

            await messageBus.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(2));

            var messages = await ReceiveMessagesForSubscriptionAsync(subscription, true);
            Assert.Equal(1, aircraftLandedHandler.MessageCount);
            Assert.Single(messages.Where(m => IsMatchingAircraftId<AircraftLanded>(m, aircraftlandedEvent.AircraftId)));
        }

        [Theory]
        [InlineData(null, AuthenticationType.ConnectionString)]
        [InlineData("MyMessageType", AuthenticationType.ConnectionString)]
        public async Task PublishesEventWithMessageType(string messageTypePropertyName, AuthenticationType authenticationType)
        {
            var subscription = nameof(PublishesEventWithMessageType);
            await CreateSubscriptionAsync(subscription);
            var aircraftlandedEvent = new AircraftLanded { AircraftId = Guid.NewGuid().ToString() };
            var eventObject = new Message<IEvent>(aircraftlandedEvent);

            var options = new MessageBusOptions();
            if (messageTypePropertyName is not null)
            {
                options.MessageTypePropertyName = messageTypePropertyName;
            }

            var sut = BuildAzureServiceBusClient(authenticationType, subscription);
            await sut.ConfigureAsync(options);
            await sut.PublishAsync(eventObject);

            var matchingMessages = (await ReceiveMessagesForSubscriptionAsync(subscription))
                .Where(m => IsMatchingAircraftId<AircraftLanded>(m, aircraftlandedEvent.AircraftId));

            Assert.Single(matchingMessages);
            Assert.Equal(nameof(AircraftLanded), matchingMessages.First().ApplicationProperties[messageTypePropertyName ?? "MessageType"]);
            Assert.False(matchingMessages.First().ApplicationProperties.TryGetValue("MessageVersion", out var _));
        }
        
        [Theory]
        [InlineData(null, AuthenticationType.ConnectionString)]
        [InlineData("MyMessageVersion", AuthenticationType.ConnectionString)]
        public async Task PublishesEventWithMessageVersion(string messageVersionPropertyName, AuthenticationType authenticationType)
        {
            var subscription = nameof(PublishesEventWithMessageVersion);
            await CreateSubscriptionAsync(subscription);
            var aircraftlandedEvent = new Models.V2.AircraftLanded { AircraftId = Guid.NewGuid().ToString() };
            var eventObject = new Message<IEvent>(aircraftlandedEvent);

            var options = new MessageBusOptions();
            if (messageVersionPropertyName is not null)
            {
                options.MessageVersionPropertyName = messageVersionPropertyName;
            }

            var sut = BuildAzureServiceBusClient(authenticationType, subscription);
            await sut.ConfigureAsync(options);
            await sut.PublishAsync(eventObject);

            var matchingMessages = (await ReceiveMessagesForSubscriptionAsync(subscription))
                .Where(m => IsMatchingAircraftId<Models.V2.AircraftLanded>(m, aircraftlandedEvent.AircraftId));

            Assert.Single(matchingMessages);
            Assert.Equal(nameof(Models.V2.AircraftLanded), matchingMessages.First().ApplicationProperties["MessageType"]);
            Assert.Equal(2, matchingMessages.First().ApplicationProperties[messageVersionPropertyName ?? "MessageVersion"]);
        }
    }
}
