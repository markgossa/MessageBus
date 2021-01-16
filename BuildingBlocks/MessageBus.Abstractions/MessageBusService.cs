using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public class MessageBusService : IMessageBusService
    {
        private readonly IMessageBusHandlerResolver _messageBusHandlerResolver;
        private readonly IMessageBusAdmin _messageBusAdmin;
        private readonly IMessageBusClient _messageBusClient;

        public MessageBusService(IMessageBusHandlerResolver messageBusHandlerResolver,
            IMessageBusAdmin messageBusAdmin, IMessageBusClient messageBusClient)
        {
            _messageBusHandlerResolver = messageBusHandlerResolver;
            _messageBusAdmin = messageBusAdmin;
            _messageBusClient = messageBusClient;
        }

        public Task StartAsync() => Task.CompletedTask;

        public Task ConfigureAsync() 
            => _messageBusAdmin.ConfigureAsync(_messageBusHandlerResolver.GetMessageHandlers());
    }
}
