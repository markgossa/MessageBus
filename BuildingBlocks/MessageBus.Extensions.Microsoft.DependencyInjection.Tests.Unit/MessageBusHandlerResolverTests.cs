using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit
{
    public class MessageBusHandlerResolverTests
    {
        [Fact]
        public void MessageBusHandlerResolverReturnsMessageHandlerInstanceForGivenMessageType()
        {
            var services = new ServiceCollection().SubscribeToMessage<AircraftLanded, AircraftLandedHandler>()
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();

            var sut = new MessageBusHandlerResolver(services);
            var handler = sut.Resolve(typeof(AircraftLanded));

            Assert.NotNull(handler);
            Assert.IsType<AircraftLandedHandler>(handler);

            typeof(AircraftLandedHandler).GetMethod("Handle").Invoke(handler, new object[] { new AircraftLanded() });
        }
    }
}
