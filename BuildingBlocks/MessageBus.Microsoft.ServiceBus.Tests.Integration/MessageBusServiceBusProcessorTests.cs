using Azure.Messaging.ServiceBus;
using MessageBus.Microsoft.ServiceBus.Tests.Integration.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusServiceBusProcessorTests : MessageBusServiceBusProcessorTestsBase
    {
        [Fact]
        public async Task CreatesMessageBusServiceBusProcessorAsync()
        {
            var sut = new MessageBusServiceBusProcessor();
            await sut.StartAsync();

            await SendMessage(BuildAircraftTakenOffEvent());
        }
    }
}
