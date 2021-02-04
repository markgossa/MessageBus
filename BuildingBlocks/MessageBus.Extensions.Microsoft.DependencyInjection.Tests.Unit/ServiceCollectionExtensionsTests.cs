using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Handlers;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Models.Events;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ServiceBus1.Tests.Unit
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void SubscribesToMessage()
        {
            var services = new ServiceCollection();
            services.SubscribeToMessage(typeof(AircraftTakenOff), typeof(AircraftTakenOffHandler));

            var service = services.BuildServiceProvider().GetRequiredService<IMessageHandler<AircraftTakenOff>>();

            Assert.NotNull(service);
            Assert.IsType<AircraftTakenOffHandler>(service);
        }
        
        [Fact]
        public void SubscribesToMessageUsingGenerics()
        {
            var services = new ServiceCollection();
            services.SubscribeToMessage<AircraftLanded, AircraftLandedHandler>();

            var service = services.BuildServiceProvider().GetRequiredService<IMessageHandler<AircraftLanded>>();

            Assert.NotNull(service);
            Assert.IsType<AircraftLandedHandler>(service);
        }
        
        [Fact]
        public void AddMessageBusCreatesAndRegistersIMessageBusService()
        {
            var services = CreateServiceCollection();
            var mockMessageBusAdmin = new Mock<IMessageBusAdminClient>();
            var mockMessageBusClient = new Mock<IMessageBusClient>();
            var actualServices = services.AddMessageBus(mockMessageBusAdmin.Object, mockMessageBusClient.Object);

            var messageBusService = services.BuildServiceProvider().GetService<IMessageBus>();

            Assert.NotNull(messageBusService);
            Assert.Equal(services, actualServices);
        }
        
        [Fact]
        public async Task AddMessageBusAsyncCreatesAndRegistersIMessageBusServiceUsingIMessageBusClientBuilder()
        {
            var services = CreateServiceCollection();
            var mockMessageBusClientBuilder = new Mock<IMessageBusClientBuilder>();
            var actualServices = await services.AddMessageBusAsync(mockMessageBusClientBuilder.Object);

            var messageBusService = services.BuildServiceProvider().GetService<IMessageBus>();

            Assert.NotNull(messageBusService);
            Assert.Equal(services, actualServices);
        }
        
        [Fact]
        public void AddMessageBusCreatesAndRegistersIMessageBusServiceUsingIMessageBusClientBuilder()
        {
            var services = CreateServiceCollection();
            var mockMessageBusClientBuilder = new Mock<IMessageBusClientBuilder>();
            var actualServices = services.AddMessageBus(mockMessageBusClientBuilder.Object);

            var messageBusService = services.BuildServiceProvider().GetService<IMessageBus>();

            Assert.NotNull(messageBusService);
            Assert.Equal(services, actualServices);
        }

        private static IServiceCollection CreateServiceCollection()
        {
            IServiceCollection services = new ServiceCollection();
            return services.SubscribeToMessage<AircraftTakenOff, AircraftTakenOffHandler>()
                .SubscribeToMessage<AircraftLanded, AircraftLandedHandler>()
                .AddSingleton<IService1, Service1>();
        }
    }
}
