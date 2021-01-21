using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MessageBus.Abstractions.Tests.Unit")]

namespace MessageBus.Abstractions
{
    public class MessageBusReceiver : IMessageBusReceiver
    {
        private readonly IMessageBusHandlerResolver _messageBusHandlerResolver;
        private readonly IMessageBusAdminClient _messageBusAdmin;
        private readonly IMessageBusClient _messageBusClient;

        public MessageBusReceiver(IMessageBusHandlerResolver messageBusHandlerResolver,
            IMessageBusAdminClient messageBusAdmin, IMessageBusClient messageBusClient)
        {
            _messageBusHandlerResolver = messageBusHandlerResolver;
            _messageBusAdmin = messageBusAdmin;
            _messageBusClient = messageBusClient;
        }

        public async Task StartAsync() => await _messageBusClient.StartAsync();

        public async Task ConfigureAsync()
            => await _messageBusAdmin.ConfigureAsync(_messageBusHandlerResolver.GetMessageHandlers());

        internal async Task HandleMessageAsync(string messageContents, string messageType)
        {
            var handler = _messageBusHandlerResolver.Resolve(messageType);
            await (InvokeHandler(messageContents, handler) as Task);
        }

        private static object InvokeHandler(string messageContents, object handler)
        {
            const string handlerHandleMethodName = "HandleAsync";

            var message = DeserializeMessage(messageContents, GetMessageTypeFromHandler(handler));
            return handler.GetType().GetMethod(handlerHandleMethodName).Invoke(handler, new object[] { message });
        }

        private static object DeserializeMessage(string messageContents, System.Type messageTypeType) 
            => JsonSerializer.Deserialize(messageContents, messageTypeType);

        private static System.Type GetMessageTypeFromHandler(object handler) 
            => handler.GetType().GetInterfaces()
                .First(i => i.Name.Contains(typeof(IHandleMessages<>).Name))
                .GenericTypeArguments.First();
    }
}
