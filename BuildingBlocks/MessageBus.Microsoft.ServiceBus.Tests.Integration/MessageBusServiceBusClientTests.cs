using MessageBus.Microsoft.ServiceBus.Tests.Integration.Handlers;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusServiceBusClientTests : MessageBusServiceTestsBase
    {
        [Fact]
        public async Task CanCreateAListenerUsingConnectionString()
        {
            var mockTestHandler = new Mock<ITestHandler>();
            var sut = new MessageBusServiceBusClient(_connectionString, _topic, _subscription);
            await SendMessage(BuildAircraftTakenOffEvent());

            sut.AddMessageHandler(mockTestHandler.Object.MessageHandler);
            sut.AddErrorMessageHandler(mockTestHandler.Object.ErrorMessageHandler);
            await sut.StartAsync();
            
            await Task.Delay(TimeSpan.FromSeconds(1));

            mockTestHandler.Verify(m => m.MessageHandler(It.IsAny<EventArgs>()), Times.Once);
        }

        //[Fact]
        //public async Task CanCreateAListenerUsingManagedIdenetity()
        //{
        //    var sut = new MessageBusServiceBusClient();
        //}
    }
}
