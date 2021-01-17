using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using System.Linq;
using Xunit;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit
{
    public class MessageBusHandlerResolverTests : MessageBusHandlerResolverTestsBase
    {
        [Fact]
        public void MessageBusHandlerResolverReturnsMessageHandlerInstanceForGivenMessageType()
        {
            var sut = new MessageBusHandlerResolver(BuildServiceCollection());
            var handler = sut.Resolve(typeof(AircraftLanded));

            Assert.NotNull(handler);
            Assert.IsType<AircraftLandedHandler>(handler);

            typeof(AircraftLandedHandler).GetMethod("HandleAsync").Invoke(handler, new object[] { new AircraftLanded() });
        }

        [Fact]
        public void MessageBusHandlerResolverReturnsAllRegisteredMessageHandlers()
        {
            var sut = new MessageBusHandlerResolver(BuildServiceCollection());
            var handlers = sut.GetMessageHandlers();

            Assert.Equal(2, handlers.Count());
            Assert.Equal(typeof(AircraftLandedHandler), handlers.ElementAt(0));
            Assert.Equal(typeof(AircraftTakenOffHandler), handlers.ElementAt(1));
        }
    }
}
