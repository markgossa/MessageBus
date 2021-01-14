using MessageBus.Abstractions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusServiceBusProcessorTests : MessageBusServiceBusProcessorTestsBase
    {
        [Fact]
        public async Task CreatesMessageBusServiceBusProcessorAsync()
        {
            var mockMessageBusHandlerResolver = new Mock<IMessageBusHandlerResolver>();
            var sut = new MessageBusServiceBusProcessor(mockMessageBusHandlerResolver.Object);
            await sut.StartAsync();

            await SendMessage(BuildAircraftTakenOffEvent());
        }
    }
}
