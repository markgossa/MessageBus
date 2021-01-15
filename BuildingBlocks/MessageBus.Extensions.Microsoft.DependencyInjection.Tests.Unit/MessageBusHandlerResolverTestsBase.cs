using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit
{
    public class MessageBusHandlerResolverTestsBase
    {
        protected static ServiceCollection BuildServiceCollection()
            => new ServiceCollection().SubscribeToMessage<AircraftLanded, AircraftLandedHandler>()
                        .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();
    }
}