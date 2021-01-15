using MessageBus.Abstractions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Integration
{
    public class MessageBusServiceTests : MessageBusServiceTestsBase
    {
        //[Fact]
        //public async Task CreatesMessageBusServiceAsync()
        //{
        //    var mockMessageBusHandlerResolver = new Mock<IMessageBusHandlerResolver>();
        //    var mockMessageBusAdmin = new Mock<IMessageBusAdmin>();
        //    var mockMessageBusClient = new Mock<IMessageBusClient>(); 
        //    var sut = new MessageBusService(mockMessageBusHandlerResolver.Object, 
        //        mockMessageBusAdmin.Object, mockMessageBusClient.Object);
            
        //    await sut.StartAsync();

        //    await SendMessage(BuildAircraftTakenOffEvent());
        //}
    }
}
