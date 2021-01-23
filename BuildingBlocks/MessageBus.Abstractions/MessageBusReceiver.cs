using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MessageBus.Abstractions.Tests.Unit")]

namespace MessageBus.Abstractions
{
    public class MessageBusReceiver : IMessageBusReceiver
    {
        private readonly IMessageBusHandlerResolver _messageBusHandlerResolver;
        private readonly IMessageBusAdminClient _messageBusAdminClient;
        private readonly IMessageBusClient _messageBusClient;

        public MessageBusReceiver(IMessageBusHandlerResolver messageBusHandlerResolver,
            IMessageBusAdminClient messageBusAdmin, IMessageBusClient messageBusClient)
        {
            _messageBusHandlerResolver = messageBusHandlerResolver;
            _messageBusAdminClient = messageBusAdmin;
            _messageBusClient = messageBusClient;
        }

        public async Task StartAsync() => await _messageBusClient.StartAsync();

        public async Task ConfigureAsync()
        {
            await _messageBusAdminClient.ConfigureAsync(_messageBusHandlerResolver.GetMessageHandlers());
            _messageBusClient.AddMessageHandler(OnMessageReceived);
            _messageBusClient.AddErrorMessageHandler(OnErrorMessageReceived);
        }

        private Task OnErrorMessageReceived(EventArgs args) => Task.CompletedTask;

        internal Task OnMessageReceived(MessageReceivedEventArgs args) => HandleMessageAsync(Encoding.UTF8.GetString(args.Message)
            , "AircraftTakenOff");

        private async Task HandleMessageAsync(string messageContents, string messageType)
        {
            var handler = _messageBusHandlerResolver.Resolve(messageType);
            await (InvokeHandler(messageContents, handler) as Task);
        }

        private static async Task InvokeHandler(string messageContents, object handler)
        {
            const string handlerHandleMethodName = "HandleAsync";

            var message = DeserializeMessage(messageContents, GetMessageTypeFromHandler(handler));
            var handlerTask = handler.GetType().GetMethod(handlerHandleMethodName).Invoke(handler, new object[] { message });

            await (handlerTask as Task);
        }

        private static object DeserializeMessage(string messageContents, Type messageTypeType) 
            => JsonSerializer.Deserialize(messageContents, messageTypeType);

        private static Type GetMessageTypeFromHandler(object handler) 
            => handler.GetType().GetInterfaces()
                .First(i => i.Name.Contains(typeof(IHandleMessages<>).Name))
                .GenericTypeArguments.First();
    }
}
    