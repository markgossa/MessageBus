using MessageBus.Abstractions;
using MessageBus.Extensions.Microsoft.DependencyInjection;
using MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ServiceBus1.Tests.Unit
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddMessageBusCreatesAndRegistersIMessageBus()
        {
            var services = new ServiceCollection().AddSingleton<IService1, Service1>();
            var mockMessageBusAdmin = new Mock<IMessageBusAdminClient>();
            var mockMessageBusClient = new Mock<IMessageBusClient>();

            var messageBus = services.AddMessageBus(mockMessageBusAdmin.Object, mockMessageBusClient.Object);

            Assert.NotNull(services.BuildServiceProvider().GetService<IMessageBus>());
            Assert.IsAssignableFrom<IMessageBus>(messageBus);
        }

        [Fact]
        public void AddMessageBusCreatesAndRegistersIMessageBusUsingIMessageBusClientBuilder()
        {
            var mockMessageBusClientBuilder = new Mock<IMessageBusClientBuilder>();

            var services = new ServiceCollection().AddSingleton<IService1, Service1>();
            var messageBus = services.AddMessageBus(mockMessageBusClientBuilder.Object);

            Assert.NotNull(services.BuildServiceProvider().GetService<IMessageBus>());
            Assert.IsAssignableFrom<IMessageBus>(messageBus);
        }
    }
}
