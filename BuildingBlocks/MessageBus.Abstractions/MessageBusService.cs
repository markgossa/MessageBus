using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public class MessageBusService : IMessageBusService
    {
        public MessageBusService(IMessageBusHandlerResolver messageBusHandlerResolver, 
            IMessageBusAdmin messageBusAdmin, IMessageBusClient messageBusClient)
        {

        }

        public Task StartAsync() { return Task.CompletedTask; }
    }
}
