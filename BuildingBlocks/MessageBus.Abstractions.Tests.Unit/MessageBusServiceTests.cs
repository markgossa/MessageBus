using Moq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Abstractions.Tests.Unit
{
    public class MessageBusServiceTests
    {
        [Fact]
        public async Task Test1Async()
        {
            var mockMessageBusHandlerResolver = new Mock<IMessageBusHandlerResolver>();
            var mockMessageBusAdmin = new Mock<IMessageBusAdmin>();
            var mockMessageBusClient = new Mock<IMessageBusClient>();
            var sut = new MessageBusService(mockMessageBusHandlerResolver.Object,
                mockMessageBusAdmin.Object, mockMessageBusClient.Object);

            await sut.StartAsync();
        }
    }
}
