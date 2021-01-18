using MessageBus.Abstractions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusServiceTests : MessageBusServiceTestsBase
    {
        [Fact]
        public async Task CreatesMessageBusServiceAsync()
        {
            var mockMessageBusHandlerResolver = new Mock<IMessageBusHandlerResolver>();
            var mockMessageBusAdminClient = new Mock<IMessageBusAdminClient>();
            var mockMessageBusClient = new Mock<IMessageBusClient>();
            var sut = new MessageBusReceiver(mockMessageBusHandlerResolver.Object,
                mockMessageBusAdminClient.Object, mockMessageBusClient.Object);

            await sut.StartAsync();

            await SendMessage(BuildAircraftTakenOffEvent());
        }
    }
}
