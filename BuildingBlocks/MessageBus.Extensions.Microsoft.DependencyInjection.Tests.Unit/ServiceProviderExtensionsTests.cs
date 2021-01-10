using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using EventBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers;
using EventBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ServiceBus1.Tests.Unit
{
    public class ServiceProviderExtensionsTests
    {
        [Fact]
        public void SubscribesToMessage()
        {
            var services = new ServiceCollection();
            services.SubscribeToMessage(typeof(CarWashed), typeof(CarWashedHandler));

            var service = services.BuildServiceProvider().GetRequiredService<IHandleMessages<CarWashed>>();

            Assert.NotNull(service);
            Assert.IsType<CarWashedHandler>(service);
        }
        
        [Fact]
        public void SubscribesToMessageUsingGenerics()
        {
            var services = new ServiceCollection();
            services.SubscribeToMessage<CarWashed, CarWashedHandler>();

            var service = services.BuildServiceProvider().GetRequiredService<IHandleMessages<CarWashed>>();

            Assert.NotNull(service);
            Assert.IsType<CarWashedHandler>(service);
        }
    }
}
