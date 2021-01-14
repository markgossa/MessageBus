using MessageBus.Abstractions;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus
{
    public class MessageBusServiceBusProcessor : IMessageBusProcessor
    {
        public Task StartAsync() { return Task.CompletedTask; }
    }
}
