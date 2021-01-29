using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using System;
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
            var handler = sut.Resolve(nameof(AircraftLanded));

            Assert.NotNull(handler);
            Assert.IsType<AircraftLandedHandler>(handler);

            var messageContext = new MessageContext<AircraftLanded>(new BinaryData("Hello world!"));
            typeof(AircraftLandedHandler).GetMethod("HandleAsync").Invoke(handler, new object[] { messageContext });
        }
        
        [Fact]
        public void MessageBusHandlerResolverThrowsIfNoMessageHandlerFound()
        {
            var sut = new MessageBusHandlerResolver(BuildServiceCollection());
            Assert.Throws<MessageHandlerNotFoundException>(() => sut.Resolve("UnknownMessage"));
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
