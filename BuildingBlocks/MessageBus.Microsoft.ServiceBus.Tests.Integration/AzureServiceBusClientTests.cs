using Azure.Messaging.ServiceBus;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using Moq;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class AzureServiceBusClientTests : MessageBusReceiverTestsBase
    {
        private readonly Mock<ITestHandler> _mockTestHandler = new Mock<ITestHandler>();

        [Fact]
        public async Task CanCreateAListenerUsingConnectionString()
        {
            var subscription = nameof(CanCreateAListenerUsingConnectionString);
            await CreateSubscriptionAsync(subscription);
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            await SendMessage(aircraftTakenOffEvent);

            var sut = new AzureServiceBusClient(_connectionString, _topic, subscription);
            sut.AddMessageHandler(_mockTestHandler.Object.MessageHandler);
            sut.AddErrorMessageHandler(_mockTestHandler.Object.ErrorMessageHandler);
            await sut.StartAsync();

            await Task.Delay(TimeSpan.FromSeconds(5));
            _mockTestHandler.Verify(m => m.MessageHandler(It.Is<ProcessMessageEventArgs>(m =>
                GetAircraftIdFromMessage(m.Message) == aircraftTakenOffEvent.AircraftId)),
                Times.Once);
        }

        [Fact]
        public async Task CanCreateAListenerUsingManagedIdentity()
        {
            var _mockTestHandler = new Mock<ITestHandler>();
            _mockTestHandler.Setup(m => m.ErrorMessageHandler(It.IsAny<EventArgs>()));

            var subscription = nameof(CanCreateAListenerUsingManagedIdentity);
            await CreateSubscriptionAsync(subscription);
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            await SendMessage(aircraftTakenOffEvent);

            var sut = new AzureServiceBusClient(_hostname, _topic, subscription, _tenantId);
            sut.AddMessageHandler(_mockTestHandler.Object.MessageHandler);
            sut.AddErrorMessageHandler(_mockTestHandler.Object.ErrorMessageHandler);
            await sut.StartAsync();
            
            await Task.Delay(TimeSpan.FromSeconds(5));
            _mockTestHandler.Verify(m => m.MessageHandler(It.Is<ProcessMessageEventArgs>(m => 
                GetAircraftIdFromMessage(m.Message) == aircraftTakenOffEvent.AircraftId)), 
                Times.Once);
        }

        private static string GetAircraftIdFromMessage(ServiceBusReceivedMessage m)
        {
            var contents = Encoding.UTF8.GetString(m.Body);
            
            return JsonSerializer.Deserialize<AircraftTakenOff>(contents).AircraftId;
        }
    }
}
