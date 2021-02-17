using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MessageBus.Extensions.Microsoft.DependencyInjection.Tests.Unit
{
    public class MessageBusHostedServiceTests
    {
        [Fact]
        public async Task StartAsyncCallsMessageBus()
        {
            var mockMessageBus = new Mock<IMessageBus>();
            var mockServiceProvider = new ServiceCollection()
                .AddSingleton(mockMessageBus.Object)
                .BuildServiceProvider();

            var sut = new MessageBusHostedService(mockServiceProvider);
            await sut.StartAsync(new CancellationToken());

            mockMessageBus.Verify(m => m.StartAsync(), Times.Once);
            mockMessageBus.Verify(m => m.ConfigureAsync(), Times.Once);
        }

        [Fact]
        public async Task StopAsyncCallsMessageBus()
        {
            var mockMessageBus = new Mock<IMessageBus>();
            var mockServiceProvider = new ServiceCollection()
                .AddSingleton(mockMessageBus.Object)
                .BuildServiceProvider();

            var sut = new MessageBusHostedService(mockServiceProvider);
            await sut.StopAsync(new CancellationToken());

            mockMessageBus.Verify(m => m.StopAsync(), Times.Once);
        }
    }
}
