using Azure.Messaging.ServiceBus;
using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Utilities;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Unit
{
    public class AzureServiceBusClientTests
    {
        [Fact]
        public async Task CallsErrorMessageHandlerWithExceptionAsync()
        {
            var mockTestHandler = new Mock<ITestHandler>();
            var sut = new AzureServiceBusClient("test.servicebus.windows.net", "topic", "subscription", "12345-12345");
            sut.AddMessageHandler(mockTestHandler.Object.MessageHandler);
            sut.AddErrorMessageHandler(mockTestHandler.Object.ErrorMessageHandler);
            var args = new ProcessErrorEventArgs(new ServiceBusException("ServiceBusError",
                ServiceBusFailureReason.GeneralError), ServiceBusErrorSource.Receive, "test.servicebus.windows.net",
                "topic1", new CancellationToken());

            await sut.CallErrorMessageHandlerAsync(args);

            mockTestHandler.Verify(m => m.ErrorMessageHandler(It.Is<MessageErrorReceivedEventArgs>(e =>
                e.Exception.GetType() == typeof(ServiceBusException))), Times.Once);
        }
    }
}
