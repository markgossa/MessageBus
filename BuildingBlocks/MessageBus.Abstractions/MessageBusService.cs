using System.Threading.Tasks;

namespace MessageBus.Abstractions
{
    public class MessageBusService : IMessageBusService
    {
        private readonly IMessageBusHandlerResolver _messageBusHandlerResolver;
        private readonly IMessageBusAdminClient _messageBusAdmin;
        private readonly IMessageBusClient _messageBusClient;

        public MessageBusService(IMessageBusHandlerResolver messageBusHandlerResolver,
            IMessageBusAdminClient messageBusAdmin, IMessageBusClient messageBusClient)
        {
            _messageBusHandlerResolver = messageBusHandlerResolver;
            _messageBusAdmin = messageBusAdmin;
            _messageBusClient = messageBusClient;
        }

        public async Task StartAsync() => await _messageBusClient.StartAsync();

        public async Task ConfigureAsync()
            => await _messageBusAdmin.ConfigureAsync(_messageBusHandlerResolver.GetMessageHandlers());

        public async Task HandleMessageAsync(IEvent message)
        {
            var handler = _messageBusHandlerResolver.Resolve(message.GetType());
            await (InvokeHandler(message, handler) as Task);
        }

        private static object InvokeHandler(IEvent message, object handler)
        {
            const string handlerHandleMethodName = "HandleAsync";
            return handler.GetType().GetMethod(handlerHandleMethodName).Invoke(handler, new object[] { message });
        }
    }
}
