using MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.Extensions.Microsoft.DependencyInjection
{
    public class MessageBusHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageBusHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _serviceProvider.GetRequiredService<IMessageBus>().ConfigureAsync();
            await _serviceProvider.GetRequiredService<IMessageBus>().StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
            => await _serviceProvider.GetRequiredService<IMessageBus>().StopAsync();
    }
}
