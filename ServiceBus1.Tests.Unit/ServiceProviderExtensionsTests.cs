using Microsoft.Extensions.DependencyInjection;
using ServiceBus1.EventBus;
using ServiceBus1.Events;
using ServiceBus1.Handlers;
using Xunit;

namespace ServiceBus1.Tests.Unit
{
    public class ServiceProviderExtensionsTests
    {
        [Fact]
        public void SubscribesToMessage()
        {
            var services = new ServiceCollection();
            services.SubscribeToMessage(typeof(AircraftTakenOff), typeof(AircraftTakenOffHandler));

            var service = services.BuildServiceProvider().GetRequiredService<IHandleMessages<AircraftTakenOff>>();

            Assert.NotNull(service);
            Assert.IsType<AircraftTakenOffHandler>(service);
        }
        
        [Fact]
        public void SubscribesToMessageUsingGenerics()
        {
            var services = new ServiceCollection();
            services.SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>();

            var service = services.BuildServiceProvider().GetRequiredService<IHandleMessages<AircraftTakenOff>>();

            Assert.NotNull(service);
            Assert.IsType<AircraftTakenOffHandler>(service);
        }
    }
}
