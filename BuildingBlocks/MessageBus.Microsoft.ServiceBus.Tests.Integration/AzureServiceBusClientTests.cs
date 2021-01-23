using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class AzureServiceBusClientTests : MessageBusReceiverTestsBase
    {
        private readonly Mock<ITestHandler> mockTestHandler = new Mock<ITestHandler>();

        [Fact]
        public async Task CallsCorrectMessageHandlerUsingConnectionString()
        {
            var mockTestHandler = new Mock<ITestHandler>();
            var subscription = nameof(CallsCorrectMessageHandlerUsingConnectionString);
            var aircraftTakenOffEvent = await CreateSubscriptionAndSendMessage(subscription);

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
            var aircraftTakenOffEvent = await CreateSubscriptionAndSendMessage(subscription);

            var sut = new AzureServiceBusClient(_hostname, _topic, subscription, _tenantId);
            AddHandlers(mockTestHandler, sut);
            await sut.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(5));
            mockTestHandler.Verify(m => m.MessageHandler(It.Is<MessageReceivedEventArgs>(m =>
                GetAircraftIdFromMessage(m.Message) == aircraftTakenOffEvent.AircraftId)),
                Times.Once);
        }
    }
}
