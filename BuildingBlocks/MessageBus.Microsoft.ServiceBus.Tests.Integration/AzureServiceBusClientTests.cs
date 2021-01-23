using Azure.Messaging.ServiceBus;
using MessageBus.Abstractions;
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
        private readonly Mock<ITestHandler> mockTestHandler = new Mock<ITestHandler>();

        [Fact]
        public async Task CanCreateAListenerUsingConnectionString()
        {
            var mockTestHandler = new Mock<ITestHandler>(); 
            
            var subscription = nameof(CanCreateAListenerUsingConnectionString);
            await CreateSubscriptionAsync(subscription);
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            await SendMessage(aircraftTakenOffEvent);

            var sut = new AzureServiceBusClient(_connectionString, _topic, subscription);
            sut.AddMessageHandler(mockTestHandler.Object.MessageHandler);
            sut.AddErrorMessageHandler(mockTestHandler.Object.ErrorMessageHandler);
            await sut.StartAsync();
            
            await Task.Delay(TimeSpan.FromSeconds(5));
            mockTestHandler.Verify(m => m.MessageHandler(It.Is<MessageReceivedEventArgs>(m =>
                GetAircraftIdFromMessage(Encoding.UTF8.GetString(m.Message)) == aircraftTakenOffEvent.AircraftId)),
                Times.Once);
        }

        [Fact]
        public async Task CanCreateAListenerUsingManagedIdentity()
        {
            var mockTestHandler = new Mock<ITestHandler>();
            mockTestHandler.Setup(m => m.ErrorMessageHandler(It.IsAny<EventArgs>()));

            var subscription = nameof(CanCreateAListenerUsingManagedIdentity);
            await CreateSubscriptionAsync(subscription);
            var aircraftTakenOffEvent = BuildAircraftTakenOffEvent();
            await SendMessage(aircraftTakenOffEvent);

            var sut = new AzureServiceBusClient(_hostname, _topic, subscription, _tenantId);
            sut.AddMessageHandler(mockTestHandler.Object.MessageHandler);
            sut.AddErrorMessageHandler(mockTestHandler.Object.ErrorMessageHandler);
            await sut.StartAsync();
            
            await Task.Delay(TimeSpan.FromSeconds(5));
            mockTestHandler.Verify(m => m.MessageHandler(It.Is<MessageReceivedEventArgs>(m => 
                GetAircraftIdFromMessage(Encoding.UTF8.GetString(m.Message)) == aircraftTakenOffEvent.AircraftId)), 
                Times.Once);
        }

        //private static string GetAircraftIdFromMessage(ServiceBusReceivedMessage m)
        //{
        //    var contents = Encoding.UTF8.GetString(m.Body);

        //    return JsonSerializer.Deserialize<AircraftTakenOff>(contents).AircraftId;
        //}

        private static string GetAircraftIdFromMessage(string message) 
            => JsonSerializer.Deserialize<AircraftTakenOff>(message).AircraftId;
    }
}
