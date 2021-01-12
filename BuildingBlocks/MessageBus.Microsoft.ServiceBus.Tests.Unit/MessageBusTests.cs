using Xunit;

namespace MessageBus.Microsoft.ServiceBus.Tests.Unit
{
    public class MessageBusTests
    {
        [Fact]
        public void RegistersMessageHandlers()
        {
            ////DI Extension method:
            //var services = new ServiceCollection()
            //    .SubscribeToMessage<AircraftLanded, AircraftLandedHandler>()
            //    .AddMessageBus(new MessageBus("connectionString", "topic", "subscription")) //registers message handlers in a private field of List<IEvent, IHandleMessages<IEvent>> on MessageBus
            //    .BuildServiceProvider();


            //var sut = new ServiceBusClient("connectionString", "topic", "subscription");
            //registers message handlers in a private field of List<IEvent, IHandleMessages<IEvent>> on MessageBus
            //sut.RegisterMessageHandler<AircraftLanded, AircraftLandedHandler>();
        }
    }
}
