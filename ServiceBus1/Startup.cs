using EventBus.Extensions.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ServiceBus1.Events;
using ServiceBus1.Handlers;

namespace ServiceBus1
{
    public class Startup
    {
        public static ServiceProvider ConfigureServices()
            => new ServiceCollection()
                .SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
                .BuildServiceProvider();
    }
}
