﻿using Azure.Messaging.ServiceBus;
using MessageBus.Abstractions;
using MessageBus.Microsoft.ServiceBus.Tests.Unit.Utilities;
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
            var args = new ProcessErrorEventArgs(new ServiceBusException("ServiceBusError",
                ServiceBusFailureReason.GeneralError), ServiceBusErrorSource.Receive, "test.servicebus.windows.net",
                "topic1", new CancellationToken());

            var sut = new AzureServiceBusClient("test.servicebus.windows.net", "topic", "subscription", "12345-12345");
            sut.AddMessageHandler(mockTestHandler.Object.MessageHandler);
            sut.AddErrorMessageHandler(mockTestHandler.Object.ErrorMessageHandler);
            await sut.CallErrorMessageHandlerAsync(args);

            mockTestHandler.Verify(m => m.ErrorMessageHandler(It.Is<MessageErrorReceivedEventArgs>(e =>
                e.Exception.GetType() == typeof(ServiceBusException))), Times.Once);
        }

        [Fact]
        public void AcceptsServiceBusClientSettings()
        {
            _ = new AzureServiceBusClient("test.servicebus.windows.net", "topic", "subscription", "12345-12345",
                    new ServiceBusProcessorOptions());


            var connectionString = "Endpoint=sb://test.servicebus.windows.net/;" +
                "SharedAccessKeyName=Manage;SharedAccessKey=12345=;";
            _ = new AzureServiceBusClient(connectionString, "topic", "subscription", new ServiceBusProcessorOptions());
        }
    }
}
